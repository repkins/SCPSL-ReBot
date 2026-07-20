using ReBot.Main.AI.Movement.Pathfinding;
using ReBot.Main.AI.Spacial;
using ReBot.Navigation.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

public struct ObstacleSightPerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    struct ObstacleSightBot()
    {
        public NativeList<int> obstacleWithinFovTargets = new(Allocator.Temp);
        public NativeList<RaycastHit> obstacleRaycastResults = new(Allocator.Temp);
        public NativeList<ObstacleRef> sightObstacleRefs = new(Allocator.Temp);

        public NativeList<ObstacleRef> elevationObstacleRefs = new(Allocator.Temp);
    }

    private NativeArray<ObstacleSightBot> obstacleSightBots = new(botCount, Allocator.Temp);

    public JobHandle ProcessObstacleSighting()
    {
        var obstacleRaycastCommandsHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var cameraPosition = bot.PlayerState.CameraPosition;
            var cameraForward = bot.PlayerState.CameraForward;

            var cellsPath = PathfindingStore.CellsPath[botIdx];

            var sightObstaclePositions = new NativeList<Vector3>(Allocator.Temp);
            var sightObstacleRefs = obstacleSightBots[botIdx].sightObstacleRefs;

            var elevationObstacleRefs = obstacleSightBots[botIdx].elevationObstacleRefs;

            var receivedObstacles = bot.ObstacleState.ObstaclesByTarget;

            foreach (var pair in receivedObstacles)
            {
                var obstacleRef = pair.Value;

                if (ObstacleState.ElevationTravelObstacles.IndexOf(obstacleRef) is var elevationIdx && elevationIdx > -1)
                {
                    elevationObstacleRefs.Add(obstacleRef);
                }
                else
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                    sightObstaclePositions.Add(obstaclePosition);
                    sightObstacleRefs.Add(obstacleRef);
                }
            }

            TransformCell? prevCell = null;
            foreach (var cellInPath in cellsPath)
            {
                if (!ObstacleState.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    continue;
                }

                if (ObstacleState.ElevationTravelObstacles.IndexOf(obstacleRef) is var elevationIdx && elevationIdx > -1)
                {
                    if (!prevCell.HasValue || prevCell.Value.AdjacentCellEdges.ContainsKey(cellInPath) ||
                        NavigationMesh.ForeignConnectedCellEdges[prevCell.Value].ContainsKey(cellInPath))
                    {
                        continue;
                    }

                    elevationObstacleRefs.Add(obstacleRef);
                }
                else
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                    sightObstaclePositions.Add(obstaclePosition);
                    sightObstacleRefs.Add(obstacleRef);
                }

                prevCell = cellInPath;
            }

            var withinFovTargets = obstacleSightBots[botIdx].obstacleWithinFovTargets;
            withinFovTargets.SetCapacity(sightObstaclePositions.Length);

            var raycastCommands = new NativeArray<RaycastCommand>(sightObstaclePositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = sightObstaclePositions.AsArray(),

                outWithinFovTargets = withinFovTargets,

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(sightObstaclePositions.Length, default);

            var raycastResults = obstacleSightBots[botIdx].obstacleRaycastResults;
            raycastResults.ResizeUninitialized(raycastCommands.Length);

            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults.AsArray(), raycastCommands.Length, withinFovJobHandle);

            obstacleRaycastCommandsHandles[botIdx] = raycastCommandsHandle;
        }

        return JobHandle.CombineDependencies(obstacleRaycastCommandsHandles);
    }

    public void ProcessObstacleSightResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var withinFovTargets = obstacleSightBots[botIdx].obstacleWithinFovTargets;
            var raycastResults = obstacleSightBots[botIdx].obstacleRaycastResults;
            var sightObstacleRefs = obstacleSightBots[botIdx].sightObstacleRefs;

            var obstacles = bot.ObstacleState.ObstaclesByTarget;
            var goalPos = PathfindingStore.GoalPositions[botIdx];

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovTargets.Length; withinFovResultIdx++)
            {
                var targetIdx = withinFovTargets[withinFovResultIdx];

                var obstacleRef = sightObstacleRefs[targetIdx];

                var hit = raycastResults[targetIdx];
                if (hit.collider is null)
                {
                    if (obstacles.TryGetValue(goalPos, out var storedObstacleRef) && storedObstacleRef == obstacleRef)
                    {
                        obstacles.Remove(goalPos);
                    }
                    continue;
                }

                var cellInPath = ObstacleState.CellsByObstacle[obstacleRef];
                var obstacleTransform = NavigationMesh.CellsWithObstacles[cellInPath];
                if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(hit.collider))
                {
                    continue;
                }

                if (obstacles.ContainsKey(goalPos))
                {
                    continue;
                }

                obstacles[goalPos] = obstacleRef;
            }

            var elevationObstacleRefs = obstacleSightBots[botIdx].elevationObstacleRefs;
            var playerPosition = bot.PlayerState.Position;

            for (var elevationObstacleIdx = 0; elevationObstacleIdx < elevationObstacleRefs.Length; elevationObstacleIdx++)
            {
                var obstacleRef = elevationObstacleRefs[elevationObstacleIdx];

                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                if (playerPosition.y - obstaclePosition.y < 1f)
                {
                    if (obstacles.TryGetValue(goalPos, out var storedObstacleRef) && storedObstacleRef == obstacleRef)
                    {
                        obstacles.Remove(goalPos);
                    }
                    continue;
                }

                if (obstacles.ContainsKey(goalPos))
                {
                    continue;
                }

                obstacles[goalPos] = obstacleRef;
            }
        }
    }
}

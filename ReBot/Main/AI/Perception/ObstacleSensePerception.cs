using ReBot.Main.AI.Movement.Pathfinding;
using ReBot.Main.AI.Spacial;
using ReBot.Navigation.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

public struct ObstacleSensePerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    struct ObstacleSightBot()
    {
        public NativeList<ObstacleRef> sightObstacleRefs = new(Allocator.Temp);
        public NativeList<Vector3> sightObstacleDestinations = new(Allocator.Temp);

        public NativeList<RaycastHit> sightObstacleRaycastResults = new(Allocator.Temp);
        public NativeList<int> withinFovSightObstacles = new(Allocator.Temp);

        public NativeList<ObstacleRef> elevationObstacleRefs = new(Allocator.Temp);
        public NativeList<Vector3> elevationObstacleDestinations = new(Allocator.Temp);
    }

    private NativeArray<ObstacleSightBot> obstacleSightBots = new(botCount, Allocator.Temp);

    public JobHandle PrepareSensableObstacles()
    {
        var raycastBatchHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var cameraPosition = bot.PlayerState.CameraPosition;
            var cameraForward = bot.PlayerState.CameraForward;

            var cellsPath = PathfindingStore.CellsPath[botIdx];

            var sightObstacleRefs = obstacleSightBots[botIdx].sightObstacleRefs;
            var sightObstacleDestinations = obstacleSightBots[botIdx].sightObstacleDestinations;
            var sightObstaclePositions = new NativeList<Vector3>(Allocator.Temp);

            var elevationObstacleRefs = obstacleSightBots[botIdx].elevationObstacleRefs;
            var elevationObstacleDestinations = obstacleSightBots[botIdx].elevationObstacleDestinations;

            var receivedObstacles = bot.ObstacleState.ObstaclesByTarget;

            foreach (var pair in receivedObstacles)
            {
                pair.GetKeyValue(out var receivedGoalPos, out var receiveObstacleRef);
                
                if (ObstacleState.ElevationTravelObstacles.IndexOf(receiveObstacleRef) is var elevationIdx && elevationIdx > -1)
                {
                    elevationObstacleRefs.Add(receiveObstacleRef);
                    elevationObstacleDestinations.Add(receivedGoalPos);
                }
                else
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[receiveObstacleRef.Idx];

                    sightObstacleRefs.Add(receiveObstacleRef);
                    sightObstacleDestinations.Add(receivedGoalPos);
                    sightObstaclePositions.Add(obstaclePosition);
                }
            }

            var currentGoalPos = PathfindingStore.GoalPositions[botIdx];

            TransformCell? prevCell = null;
            foreach (var cellInPath in cellsPath)
            {
                if (ObstacleState.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    if (ObstacleState.ElevationTravelObstacles.IndexOf(obstacleRef) is var elevationIdx && elevationIdx > -1)
                    {
                        if (prevCell.HasValue && 
                            !prevCell.Value.AdjacentCellEdges.ContainsKey(cellInPath) &&
                            !NavigationMesh.ForeignConnectedCellEdges[prevCell.Value].ContainsKey(cellInPath))
                        {
                            elevationObstacleRefs.Add(obstacleRef);
                            elevationObstacleDestinations.Add(currentGoalPos);
                        }
                    }
                    else
                    {
                        var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                        sightObstacleRefs.Add(obstacleRef);
                        sightObstacleDestinations.Add(currentGoalPos);
                        sightObstaclePositions.Add(obstaclePosition);
                    }
                }

                prevCell = cellInPath;
            }

            var withinFovSightObstacles = obstacleSightBots[botIdx].withinFovSightObstacles;
            withinFovSightObstacles.SetCapacity(sightObstaclePositions.Length);

            var sightObstacleRaycastCommands = new NativeArray<RaycastCommand>(sightObstaclePositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = sightObstaclePositions.AsArray(),

                outWithinFovTargets = withinFovSightObstacles,

                outRaycastCommands = sightObstacleRaycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(sightObstaclePositions.Length, default);

            var sightObstacleRaycastResults = obstacleSightBots[botIdx].sightObstacleRaycastResults;
            sightObstacleRaycastResults.ResizeUninitialized(sightObstacleRaycastCommands.Length);

            var raycastBatchHandle = RaycastCommand.ScheduleBatch(sightObstacleRaycastCommands, sightObstacleRaycastResults.AsArray(), sightObstacleRaycastCommands.Length, withinFovJobHandle);

            raycastBatchHandles[botIdx] = raycastBatchHandle;
        }

        return JobHandle.CombineDependencies(raycastBatchHandles);
    }

    public void ProcessSensedObstacleResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var withinFovSightObstacles = obstacleSightBots[botIdx].withinFovSightObstacles;
            var sightObstacleRaycastResults = obstacleSightBots[botIdx].sightObstacleRaycastResults;

            var sightObstacleRefs = obstacleSightBots[botIdx].sightObstacleRefs;
            var sightObstacleDestinations = obstacleSightBots[botIdx].sightObstacleDestinations;

            var obstacles = bot.ObstacleState.ObstaclesByTarget;

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovSightObstacles.Length; withinFovResultIdx++)
            {
                var sightObstacleIdx = withinFovSightObstacles[withinFovResultIdx];

                var goalPos = sightObstacleDestinations[sightObstacleIdx];
                var obstacleRef = sightObstacleRefs[sightObstacleIdx];

                var hit = sightObstacleRaycastResults[sightObstacleIdx];
                if (hit.collider is null)
                {
                    if (obstacles.ContainsKey(goalPos))
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
            var elevationObstacleDestinations = obstacleSightBots[botIdx].elevationObstacleDestinations;
            var playerPosition = bot.PlayerState.Position;

            for (var elevationObstacleIdx = 0; elevationObstacleIdx < elevationObstacleDestinations.Length; elevationObstacleIdx++)
            {
                var goalPos = elevationObstacleDestinations[elevationObstacleIdx];
                var obstacleRef = elevationObstacleRefs[elevationObstacleIdx];

                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                if (Mathf.Abs(playerPosition.y - obstaclePosition.y) < 1f)
                {
                    if (obstacles.ContainsKey(goalPos))
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

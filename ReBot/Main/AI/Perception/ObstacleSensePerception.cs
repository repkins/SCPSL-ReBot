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
        public NativeList<ObstacleRef> targetObstacleRefs = new(Allocator.Temp);
        public NativeList<Vector3> targetObstacleDestinations = new(Allocator.Temp);
        public NativeList<bool> targetObstacleObstructing = new(Allocator.Temp);

        public NativeList<int> sightObstacleTargetObstacles = new(Allocator.Temp);
        public NativeList<Vector3> sightObstaclePositions = new(Allocator.Temp);
        public NativeList<RaycastHit> sightObstacleRaycastResults = new(Allocator.Temp);
        public NativeList<int> withinFovSightObstacles = new(Allocator.Temp);

        public NativeList<int> elevationObstacleTargetObstacles = new(Allocator.Temp);

        public void AddSightObstacle(in ObstacleRef obstacleRef, in Vector3 currentGoalPos, Vector3 obstaclePosition)
        {
            var targetObstacleIdx = AddTargetObstacle(obstacleRef, currentGoalPos);

            sightObstacleTargetObstacles.Add(targetObstacleIdx);
            sightObstaclePositions.Add(obstaclePosition);
            sightObstacleRaycastResults.Add(default);
        }

        public void AddElevationObstacle(in ObstacleRef obstacleRef, in Vector3 currentGoalPos)
        {
            var targetObstacleIdx = AddTargetObstacle(obstacleRef, currentGoalPos);

            elevationObstacleTargetObstacles.Add(targetObstacleIdx);
        }

        private int AddTargetObstacle(ObstacleRef obstacleRef, Vector3 currentGoalPos)
        {
            var targetObstacleIdx = targetObstacleRefs.Length;

            targetObstacleRefs.Add(obstacleRef);
            targetObstacleDestinations.Add(currentGoalPos);
            targetObstacleObstructing.Add(default);

            return targetObstacleIdx;
        }
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

            var receivedObstacles = bot.ObstacleState.ObstaclesByTarget;

            foreach (var pair in receivedObstacles)
            {
                pair.GetKeyValue(out var receivedGoalPos, out var receiveObstacleRef);

                if (ObstacleState.ElevationTravelObstacles.IndexOf(receiveObstacleRef) is var elevationIdx && elevationIdx > -1)
                {
                    obstacleSightBots[botIdx].AddElevationObstacle(receiveObstacleRef, receivedGoalPos);
                }
                else
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleSightBots[botIdx].AddSightObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
                }
            }

            var currentGoalPos = PathfindingStore.GoalPositions[botIdx];
            var cellsPath = PathfindingStore.CellsPath[botIdx];

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
                            obstacleSightBots[botIdx].AddElevationObstacle(obstacleRef, currentGoalPos);
                        }
                    }
                    else
                    {
                        var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];
                        obstacleSightBots[botIdx].AddSightObstacle(obstacleRef, currentGoalPos, obstaclePosition);
                    }
                }

                prevCell = cellInPath;
            }

            var sightObstaclePositions = obstacleSightBots[botIdx].sightObstaclePositions;

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

            var targetObstacleRefs = obstacleSightBots[botIdx].targetObstacleRefs;
            var targetObstacleObstructing = obstacleSightBots[botIdx].targetObstacleObstructing;

            var sightObstacleTargetObstacles = obstacleSightBots[botIdx].sightObstacleTargetObstacles;

            var withinFovSightObstacles = obstacleSightBots[botIdx].withinFovSightObstacles;
            var sightObstacleRaycastResults = obstacleSightBots[botIdx].sightObstacleRaycastResults;

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovSightObstacles.Length; withinFovResultIdx++)
            {
                var sightObstacleIdx = withinFovSightObstacles[withinFovResultIdx];
                var targetObstacleIdx = sightObstacleTargetObstacles[sightObstacleIdx];

                var hit = sightObstacleRaycastResults[sightObstacleIdx];
                if (hit.collider is null)
                {
                    targetObstacleObstructing[targetObstacleIdx] = false;
                    continue;
                }

                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var cellInPath = ObstacleState.CellsByObstacle[obstacleRef];
                var obstacleTransform = NavigationMesh.CellsWithObstacles[cellInPath];
                if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(hit.collider))
                {
                    continue;
                }

                targetObstacleObstructing[targetObstacleIdx] = true;
            }

            var elevationObstacleTargetObstacles = obstacleSightBots[botIdx].elevationObstacleTargetObstacles;
            var playerPosition = bot.PlayerState.Position;

            for (var elevationObstacleIdx = 0; elevationObstacleIdx < elevationObstacleTargetObstacles.Length; elevationObstacleIdx++)
            {
                var targetObstacleIdx = elevationObstacleTargetObstacles[elevationObstacleIdx];
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                if (Mathf.Abs(playerPosition.y - obstaclePosition.y) < 1f)
                {
                    targetObstacleObstructing[targetObstacleIdx] = false;
                    continue;
                }

                targetObstacleObstructing[targetObstacleIdx] = true;
            }

            var obstacles = bot.ObstacleState.ObstaclesByTarget;
            var targetObstacleDestinations = obstacleSightBots[botIdx].targetObstacleDestinations;

            for (var targetObstacleIdx = 0; targetObstacleIdx < targetObstacleRefs.Length; targetObstacleIdx++)
            {
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var goalPos = targetObstacleDestinations[targetObstacleIdx];

                if (!targetObstacleObstructing[targetObstacleIdx])
                {
                    if (!obstacles.TryGetValue(goalPos, out var storedObstacleRef) || storedObstacleRef == obstacleRef)
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

using Interactables.Interobjects;
using ReBot.Main.AI.Movement.Pathfinding;
using ReBot.Main.AI.Spacial;
using ReBot.Navigation.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

public struct ObstacleSensePerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    enum TargetObstacleStatus
    {
        Unspecified,
        Obstructing,
        Unobstructing
    }

    struct ObstacleSightBot()
    {
        public NativeList<ObstacleRef> targetObstacleRefs = new(Allocator.Temp);
        public NativeList<Vector3> targetObstacleDestinations = new(Allocator.Temp);
        public NativeList<TargetObstacleStatus> targetObstacleStatus = new(Allocator.Temp);

        public NativeList<int> sightObstacleTargetObstacles = new(Allocator.Temp);
        public NativeList<Vector3> sightObstaclePositions = new(Allocator.Temp);
        public NativeList<RaycastHit> sightObstacleRaycastResults = new(Allocator.Temp);
        public NativeList<int> withinFovSightObstacles = new(Allocator.Temp);

        public NativeList<int> elevationTravelObstacleTargetObstacles = new(Allocator.Temp);
        public NativeList<int> elevationCallObstacleSightObstacles = new(Allocator.Temp);

        public int AddSightObstacle(in ObstacleRef obstacleRef, in Vector3 currentGoalPos, Vector3 obstaclePosition)
        {
            var targetObstacleIdx = AddTargetObstacle(obstacleRef, currentGoalPos);

            var sightObstacleIdx = sightObstacleTargetObstacles.Length;

            sightObstacleTargetObstacles.Add(targetObstacleIdx);
            sightObstaclePositions.Add(obstaclePosition);
            sightObstacleRaycastResults.Add(default);

            return sightObstacleIdx;
        }

        public void AddElevationTravelObstacle(in ObstacleRef obstacleRef, in Vector3 currentGoalPos)
        {
            var targetObstacleIdx = AddTargetObstacle(obstacleRef, currentGoalPos);

            elevationTravelObstacleTargetObstacles.Add(targetObstacleIdx);
        }

        public void AddElevationCallObstacle(in ObstacleRef obstacleRef, in Vector3 currentGoalPos, in Vector3 obstaclePosition)
        {
            var sightObstacleIdx = AddSightObstacle(obstacleRef, currentGoalPos, obstaclePosition);

            elevationCallObstacleSightObstacles.Add(sightObstacleIdx);
        }

        private int AddTargetObstacle(ObstacleRef obstacleRef, Vector3 currentGoalPos)
        {
            var targetObstacleIdx = targetObstacleRefs.Length;

            targetObstacleRefs.Add(obstacleRef);
            targetObstacleDestinations.Add(currentGoalPos);
            targetObstacleStatus.Add(default);

            return targetObstacleIdx;
        }
    }

    private NativeArray<ObstacleSightBot> obstacleSightBots = new(botCount, Allocator.Temp);

    public static void Initialize()
    {
        foreach (var (cell, solidObstacle) in NavigationMesh.CellsWithSolidObstacles)
        {
            var obstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var obstaclePosition = solidObstacle.position + Vector3.up;
            ObstacleState.ObstacleLocations.Add(obstaclePosition);
            ObstacleState.ObstaclesByCell.Add(cell, obstacleRef);
            ObstacleState.CellsByObstacle.Add(obstacleRef, cell);
        }

        foreach (var (toCell, fromCell, toCallPanel, travelPanel) in NavigationMesh.ElevationObstacles)
        {
            var levelObstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var levelObstaclePosition = toCell.MeanPosition;
            ObstacleState.ObstacleLocations.Add(levelObstaclePosition);
            ObstacleState.ObstaclesByCell.Add(toCell, levelObstacleRef);
            ObstacleState.CellsByObstacle.Add(levelObstacleRef, toCell);

            var elevationIdx = ObstacleState.ElevationObstacles.Length;
            ObstacleState.ElevationObstacles.Add(levelObstacleRef);


            var travelObstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var travelObstaclePosition = toCell.MeanPosition;
            ObstacleState.ObstacleLocations.Add(travelObstaclePosition);
            ObstacleState.CellsByObstacle.Add(travelObstacleRef, toCell);

            var travelEnterPosition = fromCell.MeanPosition;
            ObstacleState.ElevationTravelObstacles.Add(travelObstacleRef);
            ObstacleState.ElevationTravelEnterLocations.Add(travelEnterPosition);
            ObstacleState.ElevationTravelInteractPanels.Add(travelPanel);


            var callObstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var callObstaclePosition = travelPanel.GetComponentInParent<ElevatorChamber>().transform.position;
            ObstacleState.ObstacleLocations.Add(callObstaclePosition);
            ObstacleState.CellsByObstacle.Add(callObstacleRef, toCell);

            var callInteractPosition = toCallPanel.GetComponent<Collider>().bounds.center;
            ObstacleState.ElevationCallObstacles.Add(callObstacleRef);
            ObstacleState.ElevationCallInteractLocations.Add(callInteractPosition);
        }
    }

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

                if (ObstacleState.ElevationTravelObstacles.Contains(receiveObstacleRef))
                {
                    obstacleSightBots[botIdx].AddElevationTravelObstacle(receiveObstacleRef, receivedGoalPos);
                }
                else if (ObstacleState.ElevationCallObstacles.Contains(receiveObstacleRef))
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleSightBots[botIdx].AddElevationCallObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
                }
                else
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleSightBots[botIdx].AddSightObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
                }
            }

            var currentGoalPos = PathfindingStore.GoalPositions[botIdx];
            var cellsPath = PathfindingStore.CellsPath[botIdx];

            for (int cellIdx = cellsPath.Count - 1; cellIdx >= 0; cellIdx--)
            {
                TransformCell cellInPath = cellsPath[cellIdx];
                TransformCell? prevCell = cellIdx > 0 ? cellsPath[cellIdx - 1] : null;

                if (ObstacleState.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    if (ObstacleState.ElevationObstacles.IndexOf(obstacleRef) is var elevationIdx && elevationIdx > -1)
                    {
                        if (prevCell.HasValue &&
                            !prevCell.Value.AdjacentCellEdges.ContainsKey(cellInPath) &&
                            !NavigationMesh.ForeignConnectedCellEdges[prevCell.Value].ContainsKey(cellInPath))
                        {
                            var travelObstacleRef = ObstacleState.ElevationTravelObstacles[elevationIdx];
                            obstacleSightBots[botIdx].AddElevationTravelObstacle(travelObstacleRef, currentGoalPos);
                        }
                        else
                        {
                            var callObstacleRef = ObstacleState.ElevationCallObstacles[elevationIdx];
                            var obstaclePosition = ObstacleState.ObstacleLocations[callObstacleRef.Idx];
                            obstacleSightBots[botIdx].AddElevationCallObstacle(callObstacleRef, currentGoalPos, obstaclePosition);
                        }
                    }
                    else
                    {
                        var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];
                        obstacleSightBots[botIdx].AddSightObstacle(obstacleRef, currentGoalPos, obstaclePosition);
                    }
                }
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
            var targetObstacleObstructing = obstacleSightBots[botIdx].targetObstacleStatus;

            var sightObstacleTargetObstacles = obstacleSightBots[botIdx].sightObstacleTargetObstacles;

            var withinFovSightObstacles = obstacleSightBots[botIdx].withinFovSightObstacles;
            var sightObstacleRaycastResults = obstacleSightBots[botIdx].sightObstacleRaycastResults;

            var elevationCallObstacleSightObstacles = obstacleSightBots[botIdx].elevationCallObstacleSightObstacles;

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovSightObstacles.Length; withinFovResultIdx++)
            {
                var sightObstacleIdx = withinFovSightObstacles[withinFovResultIdx];
                var targetObstacleIdx = sightObstacleTargetObstacles[sightObstacleIdx];
                var hit = sightObstacleRaycastResults[sightObstacleIdx];

                if (elevationCallObstacleSightObstacles.Contains(sightObstacleIdx))
                {
                    // platform (elevator) obstacle
                    if (hit.collider is null)
                    {
                        targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
                    }
                    else if (hit.collider.GetComponentInParent<ElevatorChamber>() is null)
                    {
                        continue;
                    }
                    else
                    {
                        targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                    }
                }
                else
                {
                    // solid obstacle
                    if (hit.collider is null)
                    {
                        targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                        continue;
                    }

                    var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                    var cellInPath = ObstacleState.CellsByObstacle[obstacleRef];
                    var obstacleTransform = NavigationMesh.CellsWithSolidObstacles[cellInPath];
                    if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(hit.collider))
                    {
                        continue;
                    }

                    targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
                }
            }

            var elevationTravelObstacleTargetObstacles = obstacleSightBots[botIdx].elevationTravelObstacleTargetObstacles;
            var playerPosition = bot.PlayerState.Position;

            for (var elevationTravelObstacleIdx = 0; elevationTravelObstacleIdx < elevationTravelObstacleTargetObstacles.Length; elevationTravelObstacleIdx++)
            {
                var targetObstacleIdx = elevationTravelObstacleTargetObstacles[elevationTravelObstacleIdx];
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                if (Mathf.Abs(playerPosition.y - obstaclePosition.y) < 1f)
                {
                    targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                    continue;
                }

                targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
            }

            var obstacles = bot.ObstacleState.ObstaclesByTarget;
            var targetObstacleDestinations = obstacleSightBots[botIdx].targetObstacleDestinations;

            for (var targetObstacleIdx = 0; targetObstacleIdx < targetObstacleRefs.Length; targetObstacleIdx++)
            {
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var goalPos = targetObstacleDestinations[targetObstacleIdx];

                switch (targetObstacleObstructing[targetObstacleIdx])
                {
                    case TargetObstacleStatus.Unobstructing:
                        if (!obstacles.TryGetValue(goalPos, out var storedObstacleRef) || storedObstacleRef == obstacleRef)
                        {
                            obstacles.Remove(goalPos);
                        }

                        break;
                    case TargetObstacleStatus.Obstructing:
                        if (obstacles.ContainsKey(goalPos))
                        {
                            continue;
                        }

                        obstacles[goalPos] = obstacleRef;

                        break;
                }
            }
        }
    }

    public void ProcessSharedElevationState()
    {
        for (var elevationTravelIdx = 0; elevationTravelIdx < ObstacleState.ElevationTravelInteractPanels.Count; elevationTravelIdx++)
        {
            var travelPanel = ObstacleState.ElevationTravelInteractPanels[elevationTravelIdx];

            ObstacleState.ElevationTravelInteractLocations[elevationTravelIdx] = travelPanel.GetComponent<Collider>().bounds.center;
        }
    }
}

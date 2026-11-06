using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using ReBot.Collections;
using ReBot.Main.AI.State.Movement.Pathfinding;
using ReBot.Navigation;
using ReBot.Navigation.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.State.Spacial;

public struct ObstaclePerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    enum TargetObstacleStatus
    {
        Unspecified,
        Obstructing,
        Unobstructing
    }

    struct ObstacleBot()
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

    private NativeArray<ObstacleBot> obstacleBots = NativeArray<ObstacleBot>.CreateConstructed(botCount, Allocator.Temp);

    public static void Initialize()
    {
        NavigationSystem.Instance.OnMeshesConnected += UpdateStaticState;
        NavigationSystem.Instance.OnMeshesReset += ResetStaticState;
    }

    public static void Teardown()
    {
        NavigationSystem.Instance.OnMeshesReset -= ResetStaticState;
        NavigationSystem.Instance.OnMeshesConnected -= UpdateStaticState;
    }

    private static void UpdateStaticState()
    {
        Debug.Log("Updating bot static state");

        foreach (var (cell, solidObstacle) in NavigationMesh.CellsWithSolidObstacles)
        {
            var obstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var obstaclePosition = solidObstacle.position + Vector3.up;
            var doorVariant = solidObstacle.GetComponentInParent<CheckpointDoor>() ?? solidObstacle.GetComponent<DoorVariant>();
            var doorObstaclePermissions = doorVariant.RequiredPermissions.RequiredPermissions;
            ObstacleState.ObstacleLocations.Add(obstaclePosition);
            ObstacleState.DoorObstaclePermissions.Add(doorObstaclePermissions);
            ObstacleState.ObstaclesByCell.Add(cell, obstacleRef);
            ObstacleState.CellsByObstacle.Add(obstacleRef, cell);
        }

        foreach (var (toCell, fromCell, toCallPanel, travelPanel) in NavigationMesh.ElevationObstacles)
        {
            var levelObstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var levelObstaclePosition = toCell.MeanPosition;
            ObstacleState.ObstacleLocations.Add(levelObstaclePosition);
            ObstacleState.DoorObstaclePermissions.Add(default);
            ObstacleState.ObstaclesByCell.Add(toCell, levelObstacleRef);
            ObstacleState.CellsByObstacle.Add(levelObstacleRef, toCell);

            var elevationIdx = ObstacleState.ElevationObstacles.Length;
            ObstacleState.ElevationObstacles.Add(levelObstacleRef);


            var travelObstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            var travelObstaclePosition = toCell.MeanPosition;
            ObstacleState.ObstacleLocations.Add(travelObstaclePosition);
            ObstacleState.DoorObstaclePermissions.Add(default);
            ObstacleState.CellsByObstacle.Add(travelObstacleRef, toCell);

            var travelEnterPosition = fromCell.MeanPosition;
            ObstacleState.ElevationTravelObstacles.Add(travelObstacleRef);
            ObstacleState.ElevationTravelEnterLocations.Add(travelEnterPosition);
            ObstacleState.ElevationTravelInteractPanels.Add(travelPanel);
            ObstacleState.ElevationTravelInteractLocations.Add(default);
            ObstacleState.ElevationTravelChamberLocations.Add(default);

            var callObstaclePosition = toCell.MeanPosition + Vector3.down;
            var callObstacleRef = new ObstacleRef(ObstacleState.ObstacleLocations.Length);
            ObstacleState.ObstacleLocations.Add(callObstaclePosition);
            ObstacleState.DoorObstaclePermissions.Add(default);
            ObstacleState.CellsByObstacle.Add(callObstacleRef, toCell);

            var callInteractPosition = toCallPanel.GetComponent<Collider>().bounds.center;
            ObstacleState.ElevationCallObstacles.Add(callObstacleRef);
            ObstacleState.ElevationCallInteractLocations.Add(callInteractPosition);
            ObstacleState.ElevationCallInteractPanels.Add(toCallPanel);
            ObstacleState.ElevationCallInteractAbles.Add(default);
        }

        Debug.Log("Updating bot static state finished");
    }

    private static void ResetStaticState()
    {
        Debug.Log("Resetting bot static state started");

        ObstacleState.ObstacleLocations.Clear();
        ObstacleState.DoorObstaclePermissions.Clear();
        ObstacleState.ObstaclesByCell.Clear();
        ObstacleState.CellsByObstacle.Clear();

        ObstacleState.ElevationObstacles.Clear();

        ObstacleState.ElevationTravelObstacles.Clear();
        ObstacleState.ElevationTravelEnterLocations.Clear();
        ObstacleState.ElevationTravelInteractPanels.Clear();
        ObstacleState.ElevationTravelInteractLocations.Clear();
        ObstacleState.ElevationTravelChamberLocations.Clear();

        ObstacleState.ElevationCallObstacles.Clear();
        ObstacleState.ElevationCallInteractLocations.Clear();
        ObstacleState.ElevationCallInteractPanels.Clear();
        ObstacleState.ElevationCallInteractAbles.Clear();

        Debug.Log("Resetting bot static state finished");
    }

    public void ProcessSharedElevationState()
    {
        for (var elevationTravelIdx = 0; elevationTravelIdx < ObstacleState.ElevationTravelInteractPanels.Count; elevationTravelIdx++)
        {
            var travelPanel = ObstacleState.ElevationTravelInteractPanels[elevationTravelIdx];
            var elevator = travelPanel.AssignedChamber;

            ObstacleState.ElevationTravelInteractLocations[elevationTravelIdx] = travelPanel.GetComponent<Collider>().bounds.center;
            ObstacleState.ElevationTravelChamberLocations[elevationTravelIdx] = elevator.transform.position + Vector3.up;
        }

        for (var elevationCallIdx = 0; elevationCallIdx < ObstacleState.ElevationCallInteractPanels.Count; elevationCallIdx++)
        {
            var callPanel = ObstacleState.ElevationTravelInteractPanels[elevationCallIdx];

            if (callPanel.Target is not ElevatorChamber chamber)
            {
                Debug.LogWarning($"Elevator call panel \"{callPanel}\" interaction target is not instance of {nameof(ElevatorChamber)}");
                continue;
            }

            ObstacleState.ElevationCallInteractAbles[elevationCallIdx] = chamber.IsReadyForUserInput;
        }
    }

    public JobHandle PrepareSensableObstacles()
    {
        var raycastBatchHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var cameraPosition = bot.PlayerStore.CameraPosition;
            var cameraForward = bot.PlayerStore.CameraForward;

            var receivedObstacles = bot.ObstacleStore.ObstaclesByTarget;

            foreach (var pair in receivedObstacles)
            {
                pair.GetKeyValue(out var receivedGoalPos, out var receiveObstacleRef);

                if (ObstacleState.ElevationTravelObstacles.Contains(receiveObstacleRef))
                {
                    obstacleBots[botIdx].AddElevationTravelObstacle(receiveObstacleRef, receivedGoalPos);
                }
                else if (ObstacleState.ElevationCallObstacles.Contains(receiveObstacleRef))
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleBots[botIdx].AddElevationCallObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
                }
                else
                {
                    var obstaclePosition = ObstacleState.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleBots[botIdx].AddSightObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
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
                            obstacleBots[botIdx].AddElevationTravelObstacle(travelObstacleRef, currentGoalPos);
                        }
                        else
                        {
                            var callObstacleRef = ObstacleState.ElevationCallObstacles[elevationIdx];
                            var obstaclePosition = ObstacleState.ObstacleLocations[callObstacleRef.Idx];
                            obstacleBots[botIdx].AddElevationCallObstacle(callObstacleRef, currentGoalPos, obstaclePosition);
                        }
                    }
                    else
                    {
                        var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];
                        if (obstaclePosition == currentGoalPos)
                        {
                            continue;
                        }

                        obstacleBots[botIdx].AddSightObstacle(obstacleRef, currentGoalPos, obstaclePosition + Vector3.up);
                    }
                }
            }

            var sightObstaclePositions = obstacleBots[botIdx].sightObstaclePositions;

            var withinFovSightObstacles = obstacleBots[botIdx].withinFovSightObstacles;
            withinFovSightObstacles.SetCapacity(sightObstaclePositions.Length);

            var sightObstacleRaycastCommands = new NativeArray<RaycastCommand>(sightObstaclePositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                facingDirection = cameraForward,
                targetPositions = sightObstaclePositions.AsArray(),

                outWithinFovTargets = withinFovSightObstacles,

                outRaycastCommands = sightObstacleRaycastCommands,
                sightDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(sightObstaclePositions.Length, default);

            var sightObstacleRaycastResults = obstacleBots[botIdx].sightObstacleRaycastResults;
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

            var targetObstacleRefs = obstacleBots[botIdx].targetObstacleRefs;
            var targetObstacleObstructing = obstacleBots[botIdx].targetObstacleStatus;

            var sightObstacleTargetObstacles = obstacleBots[botIdx].sightObstacleTargetObstacles;

            var withinFovSightObstacles = obstacleBots[botIdx].withinFovSightObstacles;
            var sightObstacleRaycastResults = obstacleBots[botIdx].sightObstacleRaycastResults;

            var elevationCallObstacleSightObstacles = obstacleBots[botIdx].elevationCallObstacleSightObstacles;

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovSightObstacles.Length; withinFovResultIdx++)
            {
                var sightObstacleIdx = withinFovSightObstacles[withinFovResultIdx];
                var targetObstacleIdx = sightObstacleTargetObstacles[sightObstacleIdx];
                var hit = sightObstacleRaycastResults[sightObstacleIdx];
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];

                if (elevationCallObstacleSightObstacles.Contains(sightObstacleIdx))
                {
                    // platform (elevator) obstacle
                    if (hit.collider is null)
                    {
                        targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
                    }
                    else if (hit.collider.GetComponentInParent<ElevatorDoor>() is ElevatorDoor elevatorDoor)
                    {
                        targetObstacleObstructing[targetObstacleIdx] = elevatorDoor.IsConsideredOpen() 
                            ? TargetObstacleStatus.Unobstructing 
                            : TargetObstacleStatus.Obstructing;
                    }
                    else if (hit.collider.GetComponentInParent<ElevatorChamber>() is ElevatorChamber elevatorChamber)
                    {
                        var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];
                        var elevatorTravelPanelPosition = elevatorChamber.GetComponentInChildren<ElevatorPanel>().transform.position;

                        //DrawableLines.GenerateLine(hit.point, obstaclePosition);

                        targetObstacleObstructing[targetObstacleIdx] = Mathf.Abs(elevatorTravelPanelPosition.y - obstaclePosition.y) < 2f
                            ? TargetObstacleStatus.Unobstructing
                            : TargetObstacleStatus.Obstructing;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    // solid obstacle
                    if (hit.collider is null || hit.distance < .2f)
                    {
                        targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                        continue;
                    }

                    var cellInPath = ObstacleState.CellsByObstacle[obstacleRef];
                    var obstacleTransform = NavigationMesh.CellsWithSolidObstacles[cellInPath];
                    if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(hit.collider))
                    {
                        continue;
                    }

                    targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
                }
            }

            var elevationTravelObstacleTargetObstacles = obstacleBots[botIdx].elevationTravelObstacleTargetObstacles;
            var playerPosition = bot.PlayerStore.Position;

            for (var elevationTravelObstacleIdx = 0; elevationTravelObstacleIdx < elevationTravelObstacleTargetObstacles.Length; elevationTravelObstacleIdx++)
            {
                var targetObstacleIdx = elevationTravelObstacleTargetObstacles[elevationTravelObstacleIdx];
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                // Is arrived at destination level
                if (Mathf.Abs(playerPosition.y - obstaclePosition.y) < 1f)
                {
                    targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                    continue;
                }

                targetObstacleObstructing[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
            }

            var obstacles = bot.ObstacleStore.ObstaclesByTarget;
            var targetObstacleDestinations = obstacleBots[botIdx].targetObstacleDestinations;

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

            foreach (var (oldPosition, newPosition) in bot.TerminationStore.ChangedTargetPositions)
            {
                if (!obstacles.TryGetValue(oldPosition, out var obstacleRef))
                {
                    continue;
                }

                obstacles.Remove(oldPosition);
                obstacles[newPosition] = obstacleRef;
            }
            bot.TerminationStore.ChangedTargetPositions.Clear();
        }
    }
}

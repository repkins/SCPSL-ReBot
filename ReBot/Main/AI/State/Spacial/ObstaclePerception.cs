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
        public NativeList<TargetObstacleStatus> targetObstacleStatuses = new(Allocator.Temp);

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
            targetObstacleStatuses.Add(default);

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

        foreach (var (cell, (solidObstacle, emergencyRelease)) in NavigationMesh.CellsWithSolidObstacles)
        {
            var obstacleRef = new ObstacleRef(ObstacleStore.ObstacleLocations.Length);
            var obstaclePosition = solidObstacle.position + Vector3.up;
            var doorVariant = solidObstacle.GetComponentInParent<CheckpointDoor>() ?? solidObstacle.GetComponent<DoorVariant>();
            var doorObstaclePermissions = doorVariant.RequiredPermissions.RequiredPermissions;
            var doorObstacleEmergencyReleaseLocation = emergencyRelease?.GetComponent<Collider>().bounds.center;
            ObstacleStore.ObstacleLocations.Add(obstaclePosition);
            ObstacleStore.DoorObstaclePermissions.Add(doorObstaclePermissions);
            ObstacleStore.DoorObstacleEmergencyReleaseHasPositions.Add(doorObstacleEmergencyReleaseLocation.HasValue);
            ObstacleStore.DoorObstacleEmergencyReleasePositions.Add(doorObstacleEmergencyReleaseLocation ?? default);
            ObstacleStore.ObstaclesByCell.Add(cell, obstacleRef);
            ObstacleStore.CellsByObstacle.Add(obstacleRef, cell);
        }

        foreach (var (toCell, fromCell, toCallPanel, travelPanel) in NavigationMesh.ElevationObstacles)
        {
            var levelObstacleRef = new ObstacleRef(ObstacleStore.ObstacleLocations.Length);
            var levelObstaclePosition = toCell.MeanPosition;
            ObstacleStore.ObstacleLocations.Add(levelObstaclePosition);
            ObstacleStore.DoorObstaclePermissions.Add(default);
            ObstacleStore.ObstaclesByCell.Add(toCell, levelObstacleRef);
            ObstacleStore.CellsByObstacle.Add(levelObstacleRef, toCell);

            var elevationIdx = ObstacleStore.ElevationObstacles.Length;
            ObstacleStore.ElevationObstacles.Add(levelObstacleRef);


            var travelObstacleRef = new ObstacleRef(ObstacleStore.ObstacleLocations.Length);
            var travelObstaclePosition = toCell.MeanPosition;
            ObstacleStore.ObstacleLocations.Add(travelObstaclePosition);
            ObstacleStore.DoorObstaclePermissions.Add(default);
            ObstacleStore.CellsByObstacle.Add(travelObstacleRef, toCell);

            var travelEnterPosition = fromCell.MeanPosition;
            ObstacleStore.ElevationTravelObstacles.Add(travelObstacleRef);
            ObstacleStore.ElevationTravelEnterLocations.Add(travelEnterPosition);
            ObstacleStore.ElevationTravelInteractPanels.Add(travelPanel);
            ObstacleStore.ElevationTravelInteractAbles.Add(default);
            ObstacleStore.ElevationTravelInteractLocations.Add(default);
            ObstacleStore.ElevationTravelChamberLocations.Add(default);

            var callObstaclePosition = toCell.MeanPosition + Vector3.down;
            var callObstacleRef = new ObstacleRef(ObstacleStore.ObstacleLocations.Length);
            ObstacleStore.ObstacleLocations.Add(callObstaclePosition);
            ObstacleStore.DoorObstaclePermissions.Add(default);
            ObstacleStore.CellsByObstacle.Add(callObstacleRef, toCell);

            var callInteractPosition = toCallPanel.GetComponent<Collider>().bounds.center;
            ObstacleStore.ElevationCallObstacles.Add(callObstacleRef);
            ObstacleStore.ElevationCallInteractLocations.Add(callInteractPosition);
            ObstacleStore.ElevationCallInteractPanels.Add(toCallPanel);
            ObstacleStore.ElevationCallInteractAbles.Add(default);
        }

        Debug.Log("Updating bot static state finished");
    }

    private static void ResetStaticState()
    {
        Debug.Log("Resetting bot static state started");

        ObstacleStore.ObstacleLocations.Clear();
        ObstacleStore.DoorObstaclePermissions.Clear();
        ObstacleStore.DoorObstacleEmergencyReleaseHasPositions.Clear();
        ObstacleStore.DoorObstacleEmergencyReleasePositions.Clear();
        ObstacleStore.ObstaclesByCell.Clear();
        ObstacleStore.CellsByObstacle.Clear();

        ObstacleStore.ElevationObstacles.Clear();

        ObstacleStore.ElevationTravelObstacles.Clear();
        ObstacleStore.ElevationTravelEnterLocations.Clear();
        ObstacleStore.ElevationTravelInteractPanels.Clear();
        ObstacleStore.ElevationTravelInteractAbles.Clear();
        ObstacleStore.ElevationTravelInteractLocations.Clear();
        ObstacleStore.ElevationTravelChamberLocations.Clear();

        ObstacleStore.ElevationCallObstacles.Clear();
        ObstacleStore.ElevationCallInteractLocations.Clear();
        ObstacleStore.ElevationCallInteractPanels.Clear();
        ObstacleStore.ElevationCallInteractAbles.Clear();

        Debug.Log("Resetting bot static state finished");
    }

    public void ProcessSharedElevationState()
    {
        for (var elevationTravelIdx = 0; elevationTravelIdx < ObstacleStore.ElevationTravelInteractPanels.Count; elevationTravelIdx++)
        {
            var travelPanel = ObstacleStore.ElevationTravelInteractPanels[elevationTravelIdx];
            var chamber = travelPanel.AssignedChamber;

            ObstacleStore.ElevationTravelInteractLocations[elevationTravelIdx] = travelPanel.GetComponent<Collider>().bounds.center;
            ObstacleStore.ElevationTravelChamberLocations[elevationTravelIdx] = chamber.transform.position + Vector3.up;
            ObstacleStore.ElevationTravelInteractAbles[elevationTravelIdx] = chamber.IsReadyForUserInput;
        }

        for (var elevationCallIdx = 0; elevationCallIdx < ObstacleStore.ElevationCallInteractPanels.Count; elevationCallIdx++)
        {
            var callPanel = ObstacleStore.ElevationTravelInteractPanels[elevationCallIdx];

            if (callPanel.Target is not ElevatorChamber chamber)
            {
                Debug.LogWarning($"Elevator call panel \"{callPanel}\" interaction target is not instance of {nameof(ElevatorChamber)}");
                continue;
            }

            ObstacleStore.ElevationCallInteractAbles[elevationCallIdx] = chamber.IsReadyForUserInput;
        }
    }

    public JobHandle PrepareSensableObstacles()
    {
        ProcessStoredObstacles();
        ProcessNavigationPathCells();
        var raycastBatchHandles = ProcessSightObstacles();

        return JobHandle.CombineDependencies(raycastBatchHandles);
    }

    private void ProcessStoredObstacles()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var receivedObstacles = BotStore.Bots[botIdx].ObstacleStore.ObstaclesByTarget;

            foreach (var pair in receivedObstacles)
            {
                pair.GetKeyValue(out var receivedGoalPos, out var receiveObstacleRef);

                if (ObstacleStore.ElevationTravelObstacles.Contains(receiveObstacleRef))
                {
                    obstacleBots[botIdx].AddElevationTravelObstacle(receiveObstacleRef, receivedGoalPos);
                }
                else if (ObstacleStore.ElevationCallObstacles.Contains(receiveObstacleRef))
                {
                    var obstaclePosition = ObstacleStore.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleBots[botIdx].AddElevationCallObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
                }
                else
                {
                    var obstaclePosition = ObstacleStore.ObstacleLocations[receiveObstacleRef.Idx];
                    obstacleBots[botIdx].AddSightObstacle(receiveObstacleRef, receivedGoalPos, obstaclePosition);
                }
            }
        }
    }

    private void ProcessNavigationPathCells()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var currentGoalPos = PathfindingStore.GoalPositions[botIdx];
            var cellsPath = PathfindingStore.CellsPath[botIdx];

            for (int cellIdx = cellsPath.Count - 1; cellIdx >= 0; cellIdx--)
            {
                TransformCell cell = cellsPath[cellIdx];
                TransformCell? prevCell = cellIdx > 0 ? cellsPath[cellIdx - 1] : null;

                if (ObstacleStore.ObstaclesByCell.TryGetValue(cell, out var obstacleRef))
                {
                    if (ObstacleStore.ElevationObstacles.IndexOf(obstacleRef) is var elevationIdx && elevationIdx > -1)
                    {
                        if (prevCell.HasValue &&
                            !prevCell.Value.AdjacentCellEdges.ContainsKey(cell) &&
                            !NavigationMesh.ForeignConnectedCellEdges[prevCell.Value].ContainsKey(cell))
                        {
                            var travelObstacleRef = ObstacleStore.ElevationTravelObstacles[elevationIdx];
                            obstacleBots[botIdx].AddElevationTravelObstacle(travelObstacleRef, currentGoalPos);
                        }
                        else
                        {
                            if (prevCell.HasValue)
                            {
                                var callObstacleRef = ObstacleStore.ElevationCallObstacles[elevationIdx];
                                var obstaclePosition = ObstacleStore.ObstacleLocations[callObstacleRef.Idx];
                                obstacleBots[botIdx].AddElevationCallObstacle(callObstacleRef, currentGoalPos, obstaclePosition);
                            }
                        }
                    }
                    else
                    {
                        var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];
                        if (obstaclePosition == currentGoalPos)
                        {
                            continue;
                        }

                        obstacleBots[botIdx].AddSightObstacle(obstacleRef, currentGoalPos, obstaclePosition);
                    }
                }
            }
        }
    }

    private NativeArray<JobHandle> ProcessSightObstacles()
    {
        var raycastBatchHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var cameraPosition = BotStore.Bots[botIdx].PlayerStore.CameraPosition;
            var cameraForward = BotStore.Bots[botIdx].PlayerStore.CameraForward;

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

        return raycastBatchHandles;
    }

    public void ProcessSensedObstacleResults()
    {
        ProcessSightedObstacleResults();
        ProcessSolidObstacleProximity();
        ProcessElevationTravelResults();
        ProcessObstacleStatusResults();
        ProcessObstacleChangedTargetPositionResults();
    }

    private void ProcessSightedObstacleResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var targetObstacleRefs = obstacleBots[botIdx].targetObstacleRefs;
            var targetObstacleStatuses = obstacleBots[botIdx].targetObstacleStatuses;

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
                        targetObstacleStatuses[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
                    }
                    else if (hit.collider.GetComponentInParent<ElevatorDoor>() is ElevatorDoor elevatorDoor)
                    {
                        targetObstacleStatuses[targetObstacleIdx] = elevatorDoor.IsConsideredOpen()
                            ? TargetObstacleStatus.Unobstructing
                            : TargetObstacleStatus.Obstructing;
                    }
                    else if (hit.collider.GetComponentInParent<ElevatorChamber>() is ElevatorChamber elevatorChamber)
                    {
                        var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];
                        var elevatorTravelPanelPosition = elevatorChamber.GetComponentInChildren<ElevatorPanel>().transform.position;

                        //DrawableLines.GenerateLine(hit.point, obstaclePosition);

                        targetObstacleStatuses[targetObstacleIdx] = Mathf.Abs(elevatorTravelPanelPosition.y - obstaclePosition.y) < 2f
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
                    if (hit.collider is null)
                    {
                        targetObstacleStatuses[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                        continue;
                    }

                    var cellInPath = ObstacleStore.CellsByObstacle[obstacleRef];
                    var (obstacleTransform, _) = NavigationMesh.CellsWithSolidObstacles[cellInPath];
                    if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(hit.collider))
                    {
                        continue;
                    }

                    targetObstacleStatuses[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
                }
            }
        }
    }

    private void ProcessSolidObstacleProximity()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var playerPosition = BotStore.Bots[botIdx].PlayerStore.Position;

            var sightObstacleTargetObstacles = obstacleBots[botIdx].sightObstacleTargetObstacles;
            var elevationCallObstacleSightObstacles = obstacleBots[botIdx].elevationCallObstacleSightObstacles;

            var targetObstacleRefs = obstacleBots[botIdx].targetObstacleRefs;
            var targetObstacleStatuses = obstacleBots[botIdx].targetObstacleStatuses;

            for (var sightObstacleIdx = 0; sightObstacleIdx < sightObstacleTargetObstacles.Length; sightObstacleIdx++)
            {
                if (!elevationCallObstacleSightObstacles.Contains(sightObstacleIdx))
                {
                    // solid obstacle
                    var targetObstacleIdx = sightObstacleTargetObstacles[sightObstacleIdx];
                    var obstacleRef = targetObstacleRefs[targetObstacleIdx];

                    var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];

                    if (Vector3.Distance(obstaclePosition, playerPosition) < .3f)
                    {
                        targetObstacleStatuses[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                    }
                }
            }
        }
    }

    private void ProcessElevationTravelResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var playerPosition = BotStore.Bots[botIdx].PlayerStore.Position;

            var targetObstacleRefs = obstacleBots[botIdx].targetObstacleRefs;
            var targetObstacleStatuses = obstacleBots[botIdx].targetObstacleStatuses;

            var elevationTravelObstacleTargetObstacles = obstacleBots[botIdx].elevationTravelObstacleTargetObstacles;

            for (var elevationTravelObstacleIdx = 0; elevationTravelObstacleIdx < elevationTravelObstacleTargetObstacles.Length; elevationTravelObstacleIdx++)
            {
                var targetObstacleIdx = elevationTravelObstacleTargetObstacles[elevationTravelObstacleIdx];
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];

                // Is arrived at destination level
                if (Mathf.Abs(playerPosition.y - obstaclePosition.y) < 1f)
                {
                    targetObstacleStatuses[targetObstacleIdx] = TargetObstacleStatus.Unobstructing;
                    continue;
                }

                targetObstacleStatuses[targetObstacleIdx] = TargetObstacleStatus.Obstructing;
            }
        }
    }

    private void ProcessObstacleStatusResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var targetObstacleRefs = obstacleBots[botIdx].targetObstacleRefs;
            var targetObstacleStatuses = obstacleBots[botIdx].targetObstacleStatuses;

            var obstacles = BotStore.Bots[botIdx].ObstacleStore.ObstaclesByTarget;
            var targetObstacleDestinations = obstacleBots[botIdx].targetObstacleDestinations;

            for (var targetObstacleIdx = 0; targetObstacleIdx < targetObstacleRefs.Length; targetObstacleIdx++)
            {
                var obstacleRef = targetObstacleRefs[targetObstacleIdx];
                var goalPos = targetObstacleDestinations[targetObstacleIdx];

                switch (targetObstacleStatuses[targetObstacleIdx])
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

    private void ProcessObstacleChangedTargetPositionResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var obstacles = BotStore.Bots[botIdx].ObstacleStore.ObstaclesByTarget;

            foreach (var (oldPosition, newPosition) in BotStore.Bots[botIdx].TerminationStore.ChangedTargetPositions)
            {
                if (!obstacles.TryGetValue(oldPosition, out var obstacleRef))
                {
                    continue;
                }

                obstacles.Remove(oldPosition);
                obstacles[newPosition] = obstacleRef;
            }
            BotStore.Bots[botIdx].TerminationStore.ChangedTargetPositions.Clear();
        }
    }
}

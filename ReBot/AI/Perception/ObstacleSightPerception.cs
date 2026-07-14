using ReBot.AI.Movement.Pathfinding;
using ReBot.AI.Spacial;
using ReBot.Navigation.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Perception;

public struct ObstacleSightPerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    private NativeArray<NativeList<int>> obstacleWithinFovTargetsArrays = new(botCount, Allocator.Temp);
    private NativeArray<NativeArray<RaycastHit>> obstacleRaycastResultsArrays = new(botCount, Allocator.Temp);
    private NativeArray<NativeList<ObstacleRef>> obstacleRefsArrays = new(botCount, Allocator.Temp);

    public JobHandle ProcessObstacleSighting()
    {
        var obstacleRaycastCommandsHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var cameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            var cellsPath = PathfindingStore.CellsPath[botIdx];

            var obstaclePositions = new NativeList<Vector3>(Allocator.Temp);
            var obstacleRefs = new NativeList<ObstacleRef>(Allocator.Temp);

            var receivedObstacles = ObstacleStore.ObstaclesByTarget[botIdx];

            foreach (var pair in receivedObstacles)
            {
                var obstacleRef = pair.Value;

                obstacleRefs.Add(obstacleRef);

                var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];
                obstaclePositions.Add(obstaclePosition);
            }

            foreach (var cellInPath in cellsPath)
            {
                if (!ObstacleStore.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    continue;
                }

                obstacleRefs.Add(obstacleRef);

                var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];
                obstaclePositions.Add(obstaclePosition);
            }

            var withinFovTargets = new NativeList<int>(obstaclePositions.Length, Allocator.Temp);
            var raycastCommands = new NativeArray<RaycastCommand>(obstaclePositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = obstaclePositions.AsArray(),

                outWithinFovTargets = withinFovTargets,

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(obstaclePositions.Length, default);

            var raycastResults = new NativeArray<RaycastHit>(obstaclePositions.Length, Allocator.Temp);
            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            obstacleWithinFovTargetsArrays[botIdx] = withinFovTargets;
            obstacleRaycastCommandsHandles[botIdx] = raycastCommandsHandle;
            obstacleRaycastResultsArrays[botIdx] = raycastResults;
            obstacleRefsArrays[botIdx] = obstacleRefs;
        }

        return JobHandle.CombineDependencies(obstacleRaycastCommandsHandles);
    }

    public void ProcessObstacleSightResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var withinFovTargets = obstacleWithinFovTargetsArrays[botIdx];
            var raycastResults = obstacleRaycastResultsArrays[botIdx];

            var obstacles = ObstacleStore.ObstaclesByTarget[botIdx];
            var goalPos = PathfindingStore.GoalPositions[botIdx];

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovTargets.Length; withinFovResultIdx++)
            {
                var targetIdx = withinFovTargets[withinFovResultIdx];

                var hit = raycastResults[targetIdx];
                if (hit.collider is null)
                {
                    if (obstacles.ContainsKey(goalPos))
                    {
                        obstacles.Remove(goalPos);
                    }
                    continue;
                }

                var obstacleRefs = obstacleRefsArrays[botIdx];
                var obstacleRef = obstacleRefs[targetIdx];

                var cellInPath = ObstacleStore.CellsByObstacle[obstacleRef];
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
        }
    }
}

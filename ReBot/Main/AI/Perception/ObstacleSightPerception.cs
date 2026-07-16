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
        public NativeList<ObstacleRef> targetObstacleRefs = new(Allocator.Temp);
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

            var targetObstaclePositions = new NativeList<Vector3>(Allocator.Temp);
            var targetObstacleRefs = obstacleSightBots[botIdx].targetObstacleRefs;

            var receivedObstacles = bot.ObstacleState.ObstaclesByTarget;

            foreach (var pair in receivedObstacles)
            {
                var obstacleRef = pair.Value;
                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                targetObstaclePositions.Add(obstaclePosition);
                targetObstacleRefs.Add(obstacleRef);
            }

            foreach (var cellInPath in cellsPath)
            {
                if (!ObstacleState.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    continue;
                }

                var obstaclePosition = ObstacleState.ObstacleLocations[obstacleRef.Idx];

                targetObstaclePositions.Add(obstaclePosition);
                targetObstacleRefs.Add(obstacleRef);
            }

            var withinFovTargets = obstacleSightBots[botIdx].obstacleWithinFovTargets;
            withinFovTargets.SetCapacity(targetObstaclePositions.Length);

            var raycastCommands = new NativeArray<RaycastCommand>(targetObstaclePositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = targetObstaclePositions.AsArray(),

                outWithinFovTargets = withinFovTargets,

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(targetObstaclePositions.Length, default);

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

            var obstacles = bot.ObstacleState.ObstaclesByTarget;
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

                var targetObstacleRefs = obstacleSightBots[botIdx].targetObstacleRefs;
                var obstacleRef = targetObstacleRefs[targetIdx];

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
        }
    }
}

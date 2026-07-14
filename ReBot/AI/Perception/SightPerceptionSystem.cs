using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using ReBot.AI.Movement.Pathfinding;
using ReBot.AI.Spacial;
using ReBot.AI.Termination;
using ReBot.Navigation.Mesh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Perception;

internal class SightPerceptionSystem
{
    private const float maxSightRadius = 30f;
    private static readonly LayerMask sightLayerMask = LayerMask.GetMask("Default", "Door", "Ignore Raycast");

    private static readonly int playerLayerMask = LayerMask.GetMask("Ignore Raycast");

    public static void Process()
    {
        ProcessEnemiesWithinSightRadius(out NativeArray<NativeHashSet<int>> otherEnemyPlayersInstanceIdSets, out NativeArray<NativeList<Vector3>> enemyPlayerPositionLists);

        var enemyRaycastCommandsHandles = new NativeArray<JobHandle>(enemyPlayerPositionLists.Length, Allocator.TempJob);
        var enemyRaycastResultsArrays = new NativeArray<NativeArray<RaycastHit>>(enemyPlayerPositionLists.Length, Allocator.TempJob);

        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var cameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            var raycastCommands = new NativeArray<RaycastCommand>(enemyPlayerPositions.Length, Allocator.TempJob);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = enemyPlayerPositions.AsArray(),

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(enemyPlayerPositions.Length, default);

            var raycastResults = new NativeArray<RaycastHit>(enemyPlayerPositions.Length, Allocator.TempJob);
            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            raycastCommands.Dispose(raycastCommandsHandle);

            enemyRaycastCommandsHandles[botIdx] = raycastCommandsHandle;
            enemyRaycastResultsArrays[botIdx] = raycastResults;
        }

        var enemiesRaycastCommandsHandle = JobHandle.CombineDependencies(enemyRaycastCommandsHandles);
        enemyRaycastCommandsHandles.Dispose(enemiesRaycastCommandsHandle);


        var obstacleRaycastCommandsOnPathHandles = new NativeArray<JobHandle>(ObstacleStore.ObstaclesByTarget.Count, Allocator.TempJob);
        var obstacleRaycastResultsOnPathArrays = new NativeArray<NativeArray<RaycastHit>>(ObstacleStore.ObstaclesByTarget.Count, Allocator.TempJob);
        var obstacleRefsOnPathArrays = new NativeArray<NativeList<ObstacleRef>>(ObstacleStore.ObstaclesByTarget.Count, Allocator.TempJob);

        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var cameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            var cellsPath = PathfindingStore.CellsPath[botIdx];

            var obstaclePositionsOnPath = new NativeList<Vector3>(Allocator.TempJob);
            var obstacleRefsOnPath = new NativeList<ObstacleRef>(Allocator.TempJob);

            foreach (var cellInPath in cellsPath)
            {
                if (!ObstacleStore.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    continue;
                }

                obstacleRefsOnPath.Add(obstacleRef);

                var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];
                obstaclePositionsOnPath.Add(obstaclePosition);
            }

            var raycastCommands = new NativeArray<RaycastCommand>(obstaclePositionsOnPath.Length, Allocator.TempJob);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = obstaclePositionsOnPath.AsArray(),

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(obstaclePositionsOnPath.Length, default);

            var raycastResults = new NativeArray<RaycastHit>(obstaclePositionsOnPath.Length, Allocator.TempJob);
            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            raycastCommands.Dispose(raycastCommandsHandle);

            obstacleRaycastCommandsOnPathHandles[botIdx] = raycastCommandsHandle;
            obstacleRaycastResultsOnPathArrays[botIdx] = raycastResults;
            obstacleRefsOnPathArrays[botIdx] = obstacleRefsOnPath;
        }
        
        var obstaclesRaycastCommandsHandle = JobHandle.CombineDependencies(obstacleRaycastCommandsOnPathHandles);
        obstacleRaycastCommandsOnPathHandles.Dispose(obstaclesRaycastCommandsHandle);


        var allRaycastCommandsHandle = JobHandle.CombineDependencies(enemiesRaycastCommandsHandle, obstaclesRaycastCommandsHandle);
        allRaycastCommandsHandle.Complete();


        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var raycastResults = enemyRaycastResultsArrays[botIdx];

            var otherEnemyPlayersInstanceIds = otherEnemyPlayersInstanceIdSets[botIdx];
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var enemiesWithinSight = TerminationState.EnemiesWithinSight[botIdx];
            enemiesWithinSight.Clear();

            for (var enemyIdx = 0; enemyIdx < raycastResults.Length; enemyIdx++)
            {
                var raycastHit = raycastResults[enemyIdx];
                if (raycastHit.collider is null)
                {
                    continue;
                }

                if (raycastHit.collider.GetComponentInParent<FpcStandardRoleBase>() is not FpcStandardRoleBase fpcRole
                    || otherEnemyPlayersInstanceIds.Contains(fpcRole.GetInstanceID()))
                {
                    continue;
                }

                var enemyPosition = enemyPlayerPositions[enemyIdx];

                enemiesWithinSight.Add(enemyPosition);
            }

            raycastResults.Dispose();
        }

        enemyRaycastResultsArrays.Dispose();


        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var raycastResultsOnPath = obstacleRaycastResultsOnPathArrays[botIdx];

            var obstacles = ObstacleStore.ObstaclesByTarget[botIdx];
            var goalPos = PathfindingStore.GoalPositions[botIdx];

            for (var hitIdx = 0; hitIdx < raycastResultsOnPath.Length; hitIdx++)
            {
                var hit = raycastResultsOnPath[hitIdx];
                if (hit.collider is null)
                {
                    if (obstacles.ContainsKey(goalPos))
                    {
                        obstacles.Remove(goalPos);
                    }
                    continue;
                }

                var obstacleRefsOnPath = obstacleRefsOnPathArrays[botIdx];
                var obstacleRef = obstacleRefsOnPath[hitIdx];

                var cellInPath = ObstacleStore.CellsByObstacle[obstacleRef];
                var obstacleTransform = NavigationMesh.CellsWithObstacles[cellInPath];
                if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(hit.collider))
                {
                    continue;
                }

                obstacles[goalPos] = obstacleRef;
                break;
            }
        }

        obstacleRaycastResultsOnPathArrays.Dispose();
        obstacleRefsOnPathArrays.Dispose();


        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];

            var enemiesWithinDistance = TerminationState.EnemiesWithinDistance[botIdx];
            enemiesWithinDistance.Clear();

            for (var enemyIdx = 0; enemyIdx < enemyPlayerPositions.Length; enemyIdx++)
            {
                var enemyPosition = enemyPlayerPositions[enemyIdx];

                if (Vector3.Distance(cameraPosition, enemyPosition) < 2f)
                {
                    enemiesWithinDistance.Add(new(enemyIdx));
                }
            }
        }
    }

    private static void ProcessEnemiesWithinSightRadius(out NativeArray<NativeHashSet<int>> otherEnemyPlayersInstanceIdSets, out NativeArray<NativeList<Vector3>> enemyPlayerPositionLists)
    {
        var botCount = BotStore.BotHubs.Count;
        var overlapSphereCommands = new NativeArray<OverlapSphereCommand>(botCount, Allocator.Temp);
        var overlapSphereResults = new NativeArray<ColliderHit>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var playerPosition = PlayerStore.PlayerPositions[botIdx];
            overlapSphereCommands[botIdx] = new OverlapSphereCommand(playerPosition, maxSightRadius, new(playerLayerMask));
        }

        var maxHits = 10;
        var overlapSphereHandle = OverlapSphereCommand.ScheduleBatch(overlapSphereCommands, overlapSphereResults, 1, maxHits);
        overlapSphereHandle.Complete();

        otherEnemyPlayersInstanceIdSets = new NativeArray<NativeHashSet<int>>(botCount, Allocator.Temp);
        enemyPlayerPositionLists = new NativeArray<NativeList<Vector3>>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var otherEnemyPlayersInstanceIds = new NativeHashSet<int>(maxHits, Allocator.Temp);
            var enemyPlayerPositions = new NativeList<Vector3>(Allocator.Temp);

            var playerHub = BotStore.BotHubs[botIdx];

            for (int hitIdx = 0; hitIdx < maxHits; hitIdx++)
            {
                var hit = overlapSphereResults[botIdx];
                if (hit.instanceID == 0)
                {
                    break;
                }

                if (hit.collider.GetComponentInParent<FpcStandardRoleBase>() is FpcStandardRoleBase fpcRole
                    && fpcRole.Team.GetFaction() != playerHub.GetFaction()
                    && fpcRole.Team.GetFaction() != Faction.Unclassified)
                {
                    if (otherEnemyPlayersInstanceIds.Add(fpcRole.GetInstanceID()))
                    {
                        enemyPlayerPositions.Add(fpcRole.FpcModule.Position);
                    }
                }
            }

            otherEnemyPlayersInstanceIdSets[botIdx] = otherEnemyPlayersInstanceIds;
            enemyPlayerPositionLists[botIdx] = enemyPlayerPositions;
        }
    }
}

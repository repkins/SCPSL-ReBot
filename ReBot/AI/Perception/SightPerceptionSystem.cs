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
        PrepareEnemiesWithinRadius(out NativeArray<NativeHashSet<int>> otherEnemyPlayersInstanceIdSets, out NativeArray<NativeList<Vector3>> enemyPlayerPositionLists);

        var enemiesRaycastCommandsHandle = ProcessEnemySighting(enemyPlayerPositionLists, out var enemyWithinFovTargetsArrays, out var enemyRaycastResultsArrays);
        var obstaclesRaycastCommandsHandle = ProcessObstacleSighting(out var obstacleWithinFovTargetsOnPathArrays, out var obstacleRaycastResultsOnPathArrays, out var obstacleRefsOnPathArrays);

        JobHandle.CompleteAll(ref enemiesRaycastCommandsHandle, ref obstaclesRaycastCommandsHandle);

        ProcessEnemySightResults(otherEnemyPlayersInstanceIdSets, enemyPlayerPositionLists, enemyWithinFovTargetsArrays, enemyRaycastResultsArrays);
        ProcessObstacleSightResults(obstacleWithinFovTargetsOnPathArrays, obstacleRaycastResultsOnPathArrays, obstacleRefsOnPathArrays);

        ProcessEnemiesWithinDistance(enemyPlayerPositionLists);
    }

    private static void ProcessEnemiesWithinDistance(in NativeArray<NativeList<Vector3>> enemyPlayerPositionLists)
    {
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

    private static void ProcessObstacleSightResults(
        in NativeArray<NativeList<int>> obstacleWithinFovTargetsOnPathArrays, 
        in NativeArray<NativeArray<RaycastHit>> obstacleRaycastResultsOnPathArrays, 
        in NativeArray<NativeList<ObstacleRef>> obstacleRefsOnPathArrays)
    {
        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var withinFovTargetsOnPath = obstacleWithinFovTargetsOnPathArrays[botIdx];
            var raycastResultsOnPath = obstacleRaycastResultsOnPathArrays[botIdx];

            var obstacles = ObstacleStore.ObstaclesByTarget[botIdx];
            var goalPos = PathfindingStore.GoalPositions[botIdx];

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovTargetsOnPath.Length; withinFovResultIdx++)
            {
                var targetIdx = withinFovTargetsOnPath[withinFovResultIdx];

                var hit = raycastResultsOnPath[targetIdx];
                if (hit.collider is null)
                {
                    if (obstacles.ContainsKey(goalPos))
                    {
                        obstacles.Remove(goalPos);
                    }
                    continue;
                }

                var obstacleRefsOnPath = obstacleRefsOnPathArrays[botIdx];
                var obstacleRef = obstacleRefsOnPath[targetIdx];

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
    }

    private static void ProcessEnemySightResults(
        in NativeArray<NativeHashSet<int>> otherEnemyPlayersInstanceIdSets, 
        in NativeArray<NativeList<Vector3>> enemyPlayerPositionLists, 
        in NativeArray<NativeList<int>> enemyWithinFovTargetsArrays, 
        in NativeArray<NativeArray<RaycastHit>> enemyRaycastResultsArrays)
    {
        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var withinFovTargets = enemyWithinFovTargetsArrays[botIdx];
            var raycastResults = enemyRaycastResultsArrays[botIdx];

            var otherEnemyPlayersInstanceIds = otherEnemyPlayersInstanceIdSets[botIdx];
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var enemiesWithinSight = TerminationState.EnemiesWithinSight[botIdx];
            enemiesWithinSight.Clear();

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovTargets.Length; withinFovResultIdx++)
            {
                var enemyIdx = withinFovTargets[withinFovResultIdx];

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
        }
    }

    private static JobHandle ProcessObstacleSighting(
        out NativeArray<NativeList<int>> obstacleWithinFovTargetsOnPathArrays, 
        out NativeArray<NativeArray<RaycastHit>> obstacleRaycastResultsOnPathArrays, 
        out NativeArray<NativeList<ObstacleRef>> obstacleRefsOnPathArrays)
    {
        obstacleWithinFovTargetsOnPathArrays = new NativeArray<NativeList<int>>(ObstacleStore.ObstaclesByTarget.Count, Allocator.Temp);
        var obstacleRaycastCommandsOnPathHandles = new NativeArray<JobHandle>(ObstacleStore.ObstaclesByTarget.Count, Allocator.Temp);
        obstacleRaycastResultsOnPathArrays = new NativeArray<NativeArray<RaycastHit>>(ObstacleStore.ObstaclesByTarget.Count, Allocator.Temp);
        obstacleRefsOnPathArrays = new NativeArray<NativeList<ObstacleRef>>(ObstacleStore.ObstaclesByTarget.Count, Allocator.Temp);
        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var cameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            var cellsPath = PathfindingStore.CellsPath[botIdx];

            var obstaclePositionsOnPath = new NativeList<Vector3>(Allocator.Temp);
            var obstacleRefsOnPath = new NativeList<ObstacleRef>(Allocator.Temp);

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

            var withinFovTargets = new NativeList<int>(obstaclePositionsOnPath.Length, Allocator.Temp);
            var raycastCommands = new NativeArray<RaycastCommand>(obstaclePositionsOnPath.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = obstaclePositionsOnPath.AsArray(),

                outWithinFovTargets = withinFovTargets,

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(obstaclePositionsOnPath.Length, default);

            var raycastResults = new NativeArray<RaycastHit>(obstaclePositionsOnPath.Length, Allocator.Temp);
            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            obstacleWithinFovTargetsOnPathArrays[botIdx] = withinFovTargets;
            obstacleRaycastCommandsOnPathHandles[botIdx] = raycastCommandsHandle;
            obstacleRaycastResultsOnPathArrays[botIdx] = raycastResults;
            obstacleRefsOnPathArrays[botIdx] = obstacleRefsOnPath;
        }

        return JobHandle.CombineDependencies(obstacleRaycastCommandsOnPathHandles);
    }

    private static JobHandle ProcessEnemySighting(
        in NativeArray<NativeList<Vector3>> enemyPlayerPositionLists, 
        out NativeArray<NativeList<int>> enemyWithinFovTargetsArrays, 
        out NativeArray<NativeArray<RaycastHit>> enemyRaycastResultsArrays)
    {
        enemyWithinFovTargetsArrays = new NativeArray<NativeList<int>>(enemyPlayerPositionLists.Length, Allocator.Temp);
        var enemyRaycastCommandsHandles = new NativeArray<JobHandle>(enemyPlayerPositionLists.Length, Allocator.Temp);
        enemyRaycastResultsArrays = new NativeArray<NativeArray<RaycastHit>>(enemyPlayerPositionLists.Length, Allocator.Temp);
        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var cameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            var withinFovTargets = new NativeList<int>(enemyPlayerPositions.Length, Allocator.Temp);
            var raycastCommands = new NativeArray<RaycastCommand>(enemyPlayerPositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                direction = cameraForward,
                targetPositions = enemyPlayerPositions.AsArray(),

                outWithinFovTargets = withinFovTargets,

                outRaycastCommands = raycastCommands,
                raycastDistance = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(enemyPlayerPositions.Length, default);

            var raycastResults = new NativeArray<RaycastHit>(enemyPlayerPositions.Length, Allocator.Temp);
            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            enemyWithinFovTargetsArrays[botIdx] = withinFovTargets;
            enemyRaycastCommandsHandles[botIdx] = raycastCommandsHandle;
            enemyRaycastResultsArrays[botIdx] = raycastResults;
        }

        return JobHandle.CombineDependencies(enemyRaycastCommandsHandles);
    }

    private static void PrepareEnemiesWithinRadius(out NativeArray<NativeHashSet<int>> otherEnemyPlayersInstanceIdSets, out NativeArray<NativeList<Vector3>> enemyPlayerPositionLists)
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

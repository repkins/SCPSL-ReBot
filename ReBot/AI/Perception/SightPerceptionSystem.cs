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
        var obstaclesRaycastCommandsHandle = ProcessObstacleSighting(out var obstacleWithinFovTargetsArrays, out var obstacleRaycastResultsArrays, out var obstacleRefsArrays);

        JobHandle.CompleteAll(ref enemiesRaycastCommandsHandle, ref obstaclesRaycastCommandsHandle);

        ProcessEnemySightResults(otherEnemyPlayersInstanceIdSets, enemyPlayerPositionLists, enemyWithinFovTargetsArrays, enemyRaycastResultsArrays);
        ProcessObstacleSightResults(obstacleWithinFovTargetsArrays, obstacleRaycastResultsArrays, obstacleRefsArrays);

        ProcessEnemiesWithinDistance(enemyPlayerPositionLists);
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
                var hit = overlapSphereResults[botIdx * maxHits + hitIdx];
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

    private static JobHandle ProcessObstacleSighting(
        out NativeArray<NativeList<int>> obstacleWithinFovTargetsArrays, 
        out NativeArray<NativeArray<RaycastHit>> obstacleRaycastResultsArrays, 
        out NativeArray<NativeList<ObstacleRef>> obstacleRefsArrays)
    {
        obstacleWithinFovTargetsArrays = new NativeArray<NativeList<int>>(BotStore.BotHubs.Count, Allocator.Temp);
        var obstacleRaycastCommandsHandles = new NativeArray<JobHandle>(BotStore.BotHubs.Count, Allocator.Temp);
        obstacleRaycastResultsArrays = new NativeArray<NativeArray<RaycastHit>>(BotStore.BotHubs.Count, Allocator.Temp);
        obstacleRefsArrays = new NativeArray<NativeList<ObstacleRef>>(BotStore.BotHubs.Count, Allocator.Temp);

        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
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

    private static void ProcessObstacleSightResults(
        in NativeArray<NativeList<int>> obstacleWithinFovTargetsArrays,
        in NativeArray<NativeArray<RaycastHit>> obstacleRaycastResultsArrays,
        in NativeArray<NativeList<ObstacleRef>> obstacleRefsArrays)
    {
        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
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

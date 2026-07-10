using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using ReBot.AI.Spacial;
using ReBot.AI.Termination;
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

        var raycastCommandHandles = new NativeArray<JobHandle>(enemyPlayerPositionLists.Length, Allocator.TempJob);
        var raycastResultArrays = new NativeArray<NativeArray<RaycastHit>>(enemyPlayerPositionLists.Length, Allocator.TempJob);

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
            var raycastCommandHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            raycastCommands.Dispose(raycastCommandHandle);

            raycastCommandHandles[botIdx] = raycastCommandHandle;
            raycastResultArrays[botIdx] = raycastResults;
        }

        JobHandle.CompleteAll(raycastCommandHandles);

        raycastCommandHandles.Dispose();

        for (int botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var raycastResults = raycastResultArrays[botIdx];

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

        raycastResultArrays.Dispose();

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

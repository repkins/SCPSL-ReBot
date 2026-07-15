using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

public struct EnemySightPerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    private readonly int playerLayerMask = LayerMask.GetMask("Ignore Raycast");

    private NativeArray<NativeHashSet<int>> otherEnemyPlayersInstanceIdSets = new(botCount, Allocator.Temp);
    private NativeArray<NativeList<Vector3>> enemyPlayerPositionLists = new(botCount, Allocator.Temp);

    private NativeArray<NativeList<int>> enemyWithinFovTargetsArrays = new(botCount, Allocator.Temp);
    private NativeArray<NativeArray<RaycastHit>> enemyRaycastResultsArrays = new(botCount, Allocator.Temp);

    public void PrepareEnemiesWithinRadius()
    {
        var overlapSphereCommands = new NativeArray<OverlapSphereCommand>(botCount, Allocator.Temp);
        var overlapSphereResults = new NativeArray<ColliderHit>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var playerPosition = BotStore.Bots[botIdx].PlayerState.Position;
            overlapSphereCommands[botIdx] = new OverlapSphereCommand(playerPosition, maxSightRadius, new(playerLayerMask));
        }

        var maxHits = 10;
        var overlapSphereHandle = OverlapSphereCommand.ScheduleBatch(overlapSphereCommands, overlapSphereResults, 1, maxHits);
        overlapSphereHandle.Complete();

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var otherEnemyPlayersInstanceIds = new NativeHashSet<int>(maxHits, Allocator.Temp);
            var enemyPlayerPositions = new NativeList<Vector3>(Allocator.Temp);

            var playerHub = BotStore.Bots[botIdx].Hub;

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

    public JobHandle ProcessEnemySighting()
    {
        var enemyRaycastCommandsHandles = new NativeArray<JobHandle>(enemyPlayerPositionLists.Length, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var cameraPosition = bot.PlayerState.CameraPosition;
            var cameraForward = bot.PlayerState.CameraForward;

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

    public void ProcessEnemySightResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var withinFovTargets = enemyWithinFovTargetsArrays[botIdx];
            var raycastResults = enemyRaycastResultsArrays[botIdx];

            var otherEnemyPlayersInstanceIds = otherEnemyPlayersInstanceIdSets[botIdx];
            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var enemiesWithinSight = bot.TerminationState.EnemiesWithinSight;
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

    public void ProcessEnemiesWithinDistance()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var enemyPlayerPositions = enemyPlayerPositionLists[botIdx];

            var cameraPosition = bot.PlayerState.CameraPosition;

            var enemiesWithinDistance = bot.TerminationState.EnemiesWithinDistance;
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
}

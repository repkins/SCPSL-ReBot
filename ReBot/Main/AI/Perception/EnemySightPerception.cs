using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using ReBot.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

public struct EnemySightPerception(int botCount, float maxSightRadius, LayerMask sightLayerMask)
{
    struct EnemySightBot()
    {
        public NativeHashSet<int> otherEnemyPlayersInstanceIds = new(maxHits, Allocator.Temp);
        public NativeList<Vector3> enemyPlayerPositions = new(Allocator.Temp);

        public NativeList<int> enemyWithinFovTargets = new(maxHits, Allocator.Temp);
        public NativeArray<RaycastHit> enemyRaycastResults = new(maxHits, Allocator.Temp);
    }

    private const int maxHits = 10;
    private static readonly int playerLayerMask = LayerMask.GetMask("Ignore Raycast");

    private NativeArray<EnemySightBot> enemySightBots = NativeArray<EnemySightBot>.CreateConstructed(botCount, Allocator.Temp);

    public void PrepareEnemiesWithinRadius()
    {
        var overlapSphereCommands = new NativeArray<OverlapSphereCommand>(botCount, Allocator.Temp);
        var overlapSphereResults = new NativeArray<ColliderHit>(botCount * maxHits, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var playerPosition = BotStore.Bots[botIdx].PlayerState.Position;
            overlapSphereCommands[botIdx] = new OverlapSphereCommand(playerPosition, maxSightRadius, new(playerLayerMask));
        }

        var overlapSphereHandle = OverlapSphereCommand.ScheduleBatch(overlapSphereCommands, overlapSphereResults, 1, maxHits);
        overlapSphereHandle.Complete();

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var otherEnemyPlayersInstanceIds = enemySightBots[botIdx].otherEnemyPlayersInstanceIds;
            var enemyPlayerPositions = enemySightBots[botIdx].enemyPlayerPositions;

            var playerHub = BotStore.Bots[botIdx].Hub;

            for (int hitIdx = 0; hitIdx < maxHits; hitIdx++)
            {
                var hit = overlapSphereResults[botIdx * maxHits + hitIdx];
                if (hit.instanceID == 0)
                {
                    break;
                }

                if (hit.collider.gameObject.GetComponent<PlayerRoleManager>().CurrentRole is FpcStandardRoleBase fpcRole
                    && fpcRole.Team.GetFaction() != playerHub.GetFaction()
                    && fpcRole.Team.GetFaction() != Faction.Unclassified)
                {
                    if (otherEnemyPlayersInstanceIds.Add(fpcRole.GetInstanceID()))
                    {
                        enemyPlayerPositions.Add(fpcRole.FpcModule.Position);
                    }
                }
            }
        }
    }

    public JobHandle ProcessEnemySighting()
    {
        var enemyRaycastCommandsHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var cameraPosition = bot.PlayerState.CameraPosition;
            var cameraForward = bot.PlayerState.CameraForward;

            var enemyPlayerPositions = enemySightBots[botIdx].enemyPlayerPositions;
            var withinFovTargets = enemySightBots[botIdx].enemyWithinFovTargets;
            var raycastCommands = new NativeArray<RaycastCommand>(enemyPlayerPositions.Length, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                fromPosition = cameraPosition,
                facingDirection = cameraForward,
                targetPositions = enemyPlayerPositions.AsArray(),

                outWithinFovTargets = withinFovTargets,

                outRaycastCommands = raycastCommands,
                maxSightRadius = maxSightRadius,
                raycastMask = sightLayerMask
            };
            var withinFovJobHandle = withinFovJob.Schedule(enemyPlayerPositions.Length, default);

            var raycastResults = enemySightBots[botIdx].enemyRaycastResults;
            var raycastCommandsHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, raycastCommands.Length, withinFovJobHandle);

            enemyRaycastCommandsHandles[botIdx] = raycastCommandsHandle;
        }

        return JobHandle.CombineDependencies(enemyRaycastCommandsHandles);
    }

    public void ProcessEnemySightResults()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var withinFovTargets = enemySightBots[botIdx].enemyWithinFovTargets;
            var raycastResults = enemySightBots[botIdx].enemyRaycastResults;

            var otherEnemyPlayersInstanceIds = enemySightBots[botIdx].otherEnemyPlayersInstanceIds;
            var enemyPlayerPositions = enemySightBots[botIdx].enemyPlayerPositions;

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

                if (raycastHit.collider.gameObject.GetComponent<PlayerRoleManager>()?.CurrentRole is not FpcStandardRoleBase fpcRole
                    || !otherEnemyPlayersInstanceIds.Contains(fpcRole.GetInstanceID()))
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

            var enemyPlayerPositions = enemySightBots[botIdx].enemyPlayerPositions;

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

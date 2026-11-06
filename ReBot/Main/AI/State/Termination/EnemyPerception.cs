using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.PlayableScps.Scp939;
using ReBot.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.State.Termination;

public struct EnemyPerception(int botCount, float maxSightDistance, LayerMask sightLayerMask)
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

    public static void Initialize()
    {
        AnimatedCharacterModel.OnFootstepPlayed += ProcessPlayedFootstep;
    }

    public static void Teardown()
    {
        AnimatedCharacterModel.OnFootstepPlayed -= ProcessPlayedFootstep;
    }

    public void ProcessSightDistance()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];
            var hub = bot.Hub;

            bot.TerminationStore.EnemyMaxSightDistance = hub.roleManager.CurrentRole switch
            {
                Scp939Role scp939Role => ((Scp939VisibilityController)scp939Role.VisibilityController).CurrentDetectionRange,
                _ => maxSightDistance,
            };
        }
    }

    public void PrepareEnemiesWithinRadius()
    {
        var overlapSphereCommands = new NativeArray<OverlapSphereCommand>(botCount, Allocator.Temp);
        var overlapSphereResults = new NativeArray<ColliderHit>(botCount * maxHits, Allocator.Temp);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var playerPosition = BotStore.Bots[botIdx].PlayerStore.Position;
            var sightRadius = BotStore.Bots[botIdx].TerminationStore.EnemyMaxSightDistance;
            overlapSphereCommands[botIdx] = new OverlapSphereCommand(playerPosition, sightRadius, new(playerLayerMask));
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

            var cameraPosition = bot.PlayerStore.CameraPosition;
            var cameraForward = bot.PlayerStore.CameraForward;
            var sightDistance = bot.TerminationStore.EnemyMaxSightDistance;

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
                sightDistance = sightDistance,
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

            var enemiesWithinSight = bot.TerminationStore.EnemiesWithinSight;
            enemiesWithinSight.Clear();

            for (var withinFovResultIdx = 0; withinFovResultIdx < withinFovTargets.Length; withinFovResultIdx++)
            {
                var enemySightIdx = withinFovTargets[withinFovResultIdx];

                var raycastHit = raycastResults[enemySightIdx];
                if (raycastHit.collider is null)
                {
                    continue;
                }

                if (raycastHit.collider.gameObject.GetComponent<PlayerRoleManager>()?.CurrentRole is not FpcStandardRoleBase fpcRole
                    || !otherEnemyPlayersInstanceIds.Contains(fpcRole.GetInstanceID()))
                {
                    continue;
                }

                var roleInstanceId = fpcRole.GetInstanceID();
                var enemyPosition = enemyPlayerPositions[enemySightIdx];

                if (bot.TerminationStore.ReplaceOrAddEnemy(roleInstanceId, enemyPosition, out var enemyRef))
                {
                    enemiesWithinSight.Add(enemyRef);
                }
            }
        }
    }

    public void ProcessEnemiesLostSight()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var cameraPosition = bot.PlayerStore.CameraPosition;
            var cameraForward = bot.PlayerStore.CameraForward;

            var enemyLocations = bot.TerminationStore.EnemyLocations;
            var enemyVelocities = bot.TerminationStore.EnemyVelocities;

            var enemiesWithinSight = bot.TerminationStore.EnemiesWithinSight;

            for (EnemyRef enemyRef = new(0); enemyRef.Idx < enemyLocations.Length; enemyRef.Idx++)
            {
                //DrawableLines.GenerateSphere(enemyLocations[enemyRef.Idx] + enemyVelocities[enemyRef.Idx], .1f);

                if (enemiesWithinSight.Contains(enemyRef))
                {
                    continue;
                }

                var enemyPosition = enemyLocations[enemyRef.Idx];
                var enemyVelocity = enemyVelocities[enemyRef.Idx];
                if (Vector3.Distance(cameraPosition, enemyPosition) < 1.5f
                    && Vector3.Dot(cameraForward, Vector3.Normalize((enemyPosition + enemyVelocity) - cameraPosition)) > 0.9f)
                {
                    bot.TerminationStore.RemoveEnemyAtSwapback(enemyRef);
                    enemyRef.Idx--;
                }
            }
        }
    }

    public void ProcessEnemiesWithinDistance()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var enemyLocations = bot.TerminationStore.EnemyLocations;

            var cameraPosition = bot.PlayerStore.CameraPosition;

            var enemiesWithinDistance = bot.TerminationStore.EnemiesWithinDistance;
            enemiesWithinDistance.Clear();

            for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
            {
                var enemyPosition = enemyLocations[enemyIdx];

                if (Vector3.Distance(cameraPosition, enemyPosition) < 1.5f)
                {
                    enemiesWithinDistance.Add(new(enemyIdx));
                }
            }
        }
    }

    private static void ProcessPlayedFootstep(AnimatedCharacterModel model, float dis)
    {
        if (model.OwnerHub.roleManager.CurrentRole is not FpcStandardRoleBase fpcRole
            || fpcRole.Team.GetFaction() == Faction.Unclassified)
        {
            return;
        }

        var roleInstanceId = fpcRole.GetInstanceID();

        for (int botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            if (fpcRole.Team.GetFaction() == BotStore.Bots[botIdx].Hub.GetFaction())
            {
                continue;
            }

            var enemyPosition = fpcRole.FpcModule.Position;
            var playerPosition = BotStore.Bots[botIdx].PlayerStore.Position;

            if (Vector3.Distance(enemyPosition, playerPosition) > dis)
            {
                continue;
            }

            BotStore.Bots[botIdx].TerminationStore.ReplaceOrAddEnemy(roleInstanceId, enemyPosition, out var _);
        }
    }
}

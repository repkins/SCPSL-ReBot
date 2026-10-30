using DrawableLine;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;
using ReBot.Collections;
using ReBot.Main.AI.Termination;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

public struct EnemySightPerception(int botCount, float maxSightDistance, LayerMask sightLayerMask)
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

    public void ProcessSightDistance()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];
            var hub = bot.Hub;

            bot.TerminationState.EnemyMaxSightDistance = hub.roleManager.CurrentRole switch
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
            var playerPosition = BotStore.Bots[botIdx].PlayerState.Position;
            var sightRadius = BotStore.Bots[botIdx].TerminationState.EnemyMaxSightDistance;
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

            var cameraPosition = bot.PlayerState.CameraPosition;
            var cameraForward = bot.PlayerState.CameraForward;
            var sightDistance = bot.TerminationState.EnemyMaxSightDistance;

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

            var enemyLocations = bot.TerminationState.EnemyLocations;
            var enemyVelocities = bot.TerminationState.EnemyVelocities;
            var enemyInstanceIds = bot.TerminationState.EnemyInstanceIds;

            var enemiesWithinSight = bot.TerminationState.EnemiesWithinSight;
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

                var enemyPosition = enemyPlayerPositions[enemySightIdx];
                if (enemyInstanceIds.IndexOf(fpcRole.GetInstanceID()) is var enemyIdx && enemyIdx > -1)
                {
                    var prevPosition = enemyLocations[enemyIdx];
                    enemyLocations[enemyIdx] = enemyPosition;
                    enemyVelocities[enemyIdx] = Vector3.Normalize(enemyPosition - prevPosition) * 2f;

                    bot.ObstacleState.ChangedTargetPositions.Add((prevPosition, enemyPosition));
                }
                else
                {
                    if (enemyInstanceIds.Length < enemyInstanceIds.Capacity)
                    {
                        enemyIdx = enemyInstanceIds.Length;
                        enemyInstanceIds.Add(fpcRole.GetInstanceID());
                        enemyVelocities.Add(Vector3.zero);
                        enemyLocations.Add(enemyPosition);
                    }
                }

                enemiesWithinSight.Add(new(enemyIdx));
            }
        }
    }

    public void ProcessEnemiesLostSight()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var cameraPosition = bot.PlayerState.CameraPosition;
            var cameraForward = bot.PlayerState.CameraForward;

            var enemyLocations = bot.TerminationState.EnemyLocations;
            var enemyVelocities = bot.TerminationState.EnemyVelocities;
            var enemyInstanceIds = bot.TerminationState.EnemyInstanceIds;

            var enemiesWithinSight = bot.TerminationState.EnemiesWithinSight;

            for (int enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
            {
                //DrawableLines.GenerateSphere(enemyLocations[enemyIdx] + enemyVelocities[enemyIdx], .1f);

                var enemyRef = new EnemyRef(enemyIdx);
                if (enemiesWithinSight.Contains(enemyRef))
                {
                    continue;
                }

                var enemyPosition = enemyLocations[enemyIdx];
                var enemyVelocity = enemyVelocities[enemyIdx];
                if (Vector3.Distance(cameraPosition, enemyPosition) < 1.5f
                    && Vector3.Dot(cameraForward, Vector3.Normalize((enemyPosition + enemyVelocity) - cameraPosition)) > 0.9f)
                {
                    enemyInstanceIds.RemoveAtSwapBack(enemyIdx);
                    enemyVelocities.RemoveAtSwapBack(enemyIdx);
                    enemyLocations.RemoveAtSwapBack(enemyIdx);
                    enemyIdx--;
                }
            }
        }
    }

    public void ProcessEnemiesWithinDistance()
    {
        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var enemyLocations = bot.TerminationState.EnemyLocations;

            var cameraPosition = bot.PlayerState.CameraPosition;

            var enemiesWithinDistance = bot.TerminationState.EnemiesWithinDistance;
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
}

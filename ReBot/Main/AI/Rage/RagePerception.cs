using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp096;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Rage;

public struct RagePerception(int botCount)
{
    public void Process()
    {
        for (var botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];
            var hub = bot.Hub;

            if (hub.roleManager.CurrentRole is not Scp096Role scp096Role)
            {
                continue;
            }

            bot.RageState.IsEnraged = scp096Role.IsRageState(Scp096RageState.Enraged);

            if (!scp096Role.SubroutineModule.TryGetSubroutine<Scp096TargetsTracker>(out var targetsTracker))
            {
                continue;
            }

            bot.RageState.HasTargets = targetsTracker.Targets.Count > 0;

            bot.RageState.Targets.Clear();

            foreach (var target in targetsTracker.Targets)
            {
                var targetInstanceId = target.roleManager.CurrentRole.GetInstanceID();
                if (bot.TerminationState.EnemyInstanceIds.IndexOf(targetInstanceId) is var enemyIdx && enemyIdx > -1)
                {
                    bot.TerminationState.EnemyVelocities[enemyIdx] = Vector3.Normalize(target.transform.position - bot.TerminationState.EnemyLocations[enemyIdx]) * 2f;
                    bot.TerminationState.EnemyLocations[enemyIdx] = target.transform.position;
                }
                else
                {
                    if (bot.TerminationState.EnemyInstanceIds.Length < bot.TerminationState.EnemyInstanceIds.Capacity)
                    {
                        enemyIdx = bot.TerminationState.EnemyInstanceIds.Length;
                        bot.TerminationState.EnemyInstanceIds.Add(targetInstanceId);
                        bot.TerminationState.EnemyVelocities.Add(Vector3.zero);
                        bot.TerminationState.EnemyLocations.Add(target.transform.position);
                    }
                }

                bot.RageState.Targets.Add(new(enemyIdx));
            }
        }
    }
}

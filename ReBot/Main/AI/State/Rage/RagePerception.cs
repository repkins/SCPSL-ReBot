using PlayerRoles.PlayableScps.Scp096;
using UnityEngine;

namespace ReBot.Main.AI.State.Rage;

internal struct RagePerception(int botCount)
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

            bot.RageStore.IsEnraged = scp096Role.IsRageState(Scp096RageState.Enraged);

            if (!scp096Role.SubroutineModule.TryGetSubroutine<Scp096TargetsTracker>(out var targetsTracker))
            {
                continue;
            }

            bot.RageStore.HasTargets = targetsTracker.Targets.Count > 0;

            bot.RageStore.Targets.Clear();

            foreach (var target in targetsTracker.Targets)
            {
                var targetInstanceId = target.roleManager.CurrentRole.GetInstanceID();
                var targetPosition = target.transform.position;
                var targetSensedTime = Time.time;

                if (bot.TerminationStore.ReplaceOrAddEnemy(targetInstanceId, targetPosition, targetSensedTime, out var enemyRef))
                {
                    bot.RageStore.Targets.Add(enemyRef);
                }
            }
        }
    }
}

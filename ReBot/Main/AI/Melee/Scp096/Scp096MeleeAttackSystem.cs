using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;
using System.Reflection;

namespace ReBot.Main.AI.Melee.Scp096;

internal class Scp096MeleeAttackSystem
{
    static readonly MethodInfo m_Scp096AttackAbility_OnKeyDown = AccessTools.Method(typeof(Scp096AttackAbility), "OnKeyDown", []);

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var neleeAttackPositionRef = BotStore.Bots[botIdx].Scp096MeleeState.MeleeAttackPositionRef;

            var (hasPosition, _) = neleeAttackPositionRef.Value;
            neleeAttackPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var hub = BotStore.Bots[botIdx].Hub;

            if (hub.roleManager.CurrentRole is not Scp096Role scp096Role)
            {
                DebugLog.LogWarning($"Role is not SCP 096 but has set melee attack position for player {hub}");
                continue;
            }

            if (!scp096Role.SubroutineModule.TryGetSubroutine<Scp096AttackAbility>(out var meleeAttackAbility))
            {
                DebugLog.LogWarning($"Role is SCP 096 but has no melee attack ability for player {hub}");
                continue;
            }

            m_Scp096AttackAbility_OnKeyDown.Invoke(meleeAttackAbility, []);
        }
    }
}

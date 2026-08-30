using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;
using System.Reflection;

namespace ReBot.Main.AI.Melee.Scp939;

internal class Scp939ClawAttackSystem
{
    static readonly MethodInfo m_Scp939ClawAbility_OnKeyDown = AccessTools.Method(typeof(Scp939ClawAbility), "OnKeyDown", []);

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var neleeAttackPositionRef = BotStore.Bots[botIdx].Scp939MeleeState.MeleeAttackPositionRef;

            var (hasPosition, _) = neleeAttackPositionRef.Value;
            neleeAttackPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var hub = BotStore.Bots[botIdx].Hub;

            if (hub.roleManager.CurrentRole is not Scp939Role scp939Role)
            {
                DebugLog.LogWarning($"Role is not SCP 939 but has set melee attack position for player {hub}");
                continue;
            }

            if (!scp939Role.SubroutineModule.TryGetSubroutine<Scp939ClawAbility>(out var meleeAttackAbility))
            {
                DebugLog.LogWarning($"Role is SCP 939 but has no melee attack ability for player {hub}");
                continue;
            }

            m_Scp939ClawAbility_OnKeyDown.Invoke(meleeAttackAbility, []);
        }
    }
}

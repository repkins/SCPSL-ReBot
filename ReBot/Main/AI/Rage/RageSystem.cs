using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;
using System.Reflection;

namespace ReBot.Main.AI.Rage;

internal class RageSystem
{
    static readonly MethodInfo ps_Scp096RageCycleAbility_IsKeyHeld = AccessTools.PropertySetter(typeof(Scp096RageCycleAbility), "IsKeyHeld");

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var enrageRef = BotStore.Bots[botIdx].RageState.EnrageRef;
            var wantsEnrage = enrageRef.Value;
            enrageRef.Value = default;

            var calmRef = BotStore.Bots[botIdx].RageState.CalmRef;
            var wantsCalm = calmRef.Value;
            calmRef.Value = default;

            if (!wantsEnrage && !wantsCalm)
            {
                continue;
            }

            var hub = BotStore.Bots[botIdx].Hub;

            if (hub.roleManager.CurrentRole is not Scp096Role scp096Role)
            {
                DebugLog.LogWarning($"Role is not SCP 096 but has set enraged for player {hub}");
                continue;
            }

            if (!scp096Role.SubroutineModule.TryGetSubroutine<Scp096RageCycleAbility>(out var rageCycleAbility))
            {
                DebugLog.LogWarning($"Role is SCP 096 but has none it's rage cycle ablility for player {hub}");
                continue;
            }

            ps_Scp096RageCycleAbility_IsKeyHeld.Invoke(rageCycleAbility, [true]);
        }
    }
}

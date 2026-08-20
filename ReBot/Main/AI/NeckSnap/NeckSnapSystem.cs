using HarmonyLib;
using PlayerRoles.PlayableScps.Scp173;
using System.Reflection;

namespace ReBot.Main.AI.NeckSnap;

internal class NeckSnapSystem
{
    static readonly MethodInfo m_Scp173SnapAbility_OnKeyDown = AccessTools.Method(typeof(Scp173SnapAbility), "OnKeyDown", []);

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var snapPositionRef = BotStore.Bots[botIdx].NeckSnapState.SnapPositionRef;

            var (hasPosition, _) = snapPositionRef.Value;
            snapPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var hub = BotStore.Bots[botIdx].Hub;

            if (hub.roleManager.CurrentRole is not Scp173Role scp173Role)
            {
                DebugLog.LogWarning($"Role is not SCP 173 but has set snap position for player {hub}");
                continue;
            }

            if (!scp173Role.SubroutineModule.TryGetSubroutine<Scp173SnapAbility>(out var snapAbility))
            {
                DebugLog.LogWarning($"Role is SCP 173 but has no snap ability for player {hub}");
                continue;
            }

            m_Scp173SnapAbility_OnKeyDown.Invoke(snapAbility, []);
        }
    }
}

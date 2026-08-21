using HarmonyLib;
using PlayerRoles.PlayableScps.Scp173;
using System.Reflection;

namespace ReBot.Main.AI.Blink;

internal class BlinkSystem
{
    static readonly MethodInfo m_Scp173TeleportAbility_UpdateAiming = AccessTools.Method(typeof(Scp173TeleportAbility), "UpdateAiming", [typeof(bool)]);

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var blinkToPositionRef = BotStore.Bots[botIdx].BlinkState.BlinkToPositionRef;

            var (hasPosition, _) = blinkToPositionRef.Value;
            blinkToPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var hub = BotStore.Bots[botIdx].Hub;

            if (hub.roleManager.CurrentRole is not Scp173Role scp173Role)
            {
                DebugLog.LogWarning($"Role is not SCP 173 but has set blink to position for player {hub}");
                continue;
            }

            if (!scp173Role.SubroutineModule.TryGetSubroutine<Scp173TeleportAbility>(out var teleportAbility))
            {
                DebugLog.LogWarning($"Role is SCP 173 but has none it's own teleport ablility for player {hub}");
                continue;
            }

            m_Scp173TeleportAbility_UpdateAiming.Invoke(teleportAbility, [true]);
        }
    }
}

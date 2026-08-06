using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Subroutines;

namespace ReBot.Main.AI.Capture;

internal class CaptureSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var cardiacPositionRef = BotStore.Bots[botIdx].CardiacState.CardiacPositionRef;

            var (hasPosition, _) = cardiacPositionRef.Value;
            cardiacPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var hub = BotStore.Bots[botIdx].Hub;
            if (hub.roleManager.CurrentRole is not Scp106Role scp106Role)
            {
                DebugLog.LogWarning($"Role is not SCP 106 but has set capture position for player {hub}");
                continue;
            }

            if (!scp106Role.SubroutineModule.TryGetSubroutine<Scp106Attack>(out var attackAbility))
            {
                DebugLog.LogWarning($"Role is SCP 106 but has no capture ability for player {hub}");
                continue;
            }

            hub.connectionToServer.Send<SubroutineMessage>(new(attackAbility, false));
        }
    }
}

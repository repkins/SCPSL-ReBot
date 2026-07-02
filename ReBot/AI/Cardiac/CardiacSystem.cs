using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Subroutines;

namespace ReBot.AI.Cardiac;

internal class CardiacSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < CardiacStore.CardiacPositionRefs.Count; botIdx++)
        {
            var (hasPosition, position) = CardiacStore.CardiacPositionRefs[botIdx].Value;
            if (!hasPosition)
            {
                continue;
            }

            var hub = BotStore.BotHubs[botIdx];
            if (hub.roleManager.CurrentRole is not Scp049Role scp049Role)
            {
                DebugLog.LogWarning($"Role is not SCP 049 but has set cardiac position for player {hub}");
                continue;
            }

            if (!scp049Role.SubroutineModule.TryGetSubroutine<Scp049AttackAbility>(out var attackAbility))
            {
                DebugLog.LogWarning($"Role is SCP 049 but has no attack ability for player {hub}");
                continue;
            }

            hub.connectionToServer.Send<SubroutineMessage>(new(attackAbility, false));
        }
    }
}

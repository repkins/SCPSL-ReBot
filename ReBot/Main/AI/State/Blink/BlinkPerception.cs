using PlayerRoles.PlayableScps.Scp173;

namespace ReBot.Main.AI.State.Blink;

internal struct BlinkPerception(int botCount)
{
    public void Process()
    {
        for (var botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];
            var hub = bot.Hub;

            if (hub.roleManager.CurrentRole is not Scp173Role scp173Role)
            {
                continue;
            }

            if (!scp173Role.SubroutineModule.TryGetSubroutine<Scp173ObserversTracker>(out var observersTracker))
            {
                continue;
            }

            bot.BlinkStore.IsObserved = observersTracker.IsObserved;

            if (!scp173Role.SubroutineModule.TryGetSubroutine<Scp173BlinkTimer>(out var blinkTimer))
            {
                continue;
            }

            bot.BlinkStore.RemainingSecondsToBlink = blinkTimer.RemainingBlinkCooldown;
        }
    }
}

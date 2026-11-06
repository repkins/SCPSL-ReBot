namespace ReBot.Main.AI.State.Rage;

internal class RageActionSystem
{
    static readonly string ReleaseReloadDummyActionName = $"{ActionName.Reload}->Release";
    static readonly string HoldReleaseDummyActionName = $"{ActionName.Reload}->Hold";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var enrageRef = BotStore.Bots[botIdx].RageStore.EnrageRef;
            var wantsEnrage = enrageRef.Value;
            enrageRef.Value = default;

            var calmRef = BotStore.Bots[botIdx].RageStore.CalmRef;
            var wantsCalm = calmRef.Value;
            calmRef.Value = default;

            if (!wantsEnrage && !wantsCalm)
            {
                BotStore.Bots[botIdx].ActionStore.WantSet.Add(ReleaseReloadDummyActionName);
                continue;
            }

            BotStore.Bots[botIdx].ActionStore.WantSet.Add(HoldReleaseDummyActionName);
        }
    }
}

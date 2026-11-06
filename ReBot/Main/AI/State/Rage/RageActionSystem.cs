namespace ReBot.Main.AI.State.Rage;

internal class RageActionSystem
{
    static readonly string ReleaseReloadDummyActionName = $"{ActionName.Reload}->Release";
    static readonly string HoldReleaseDummyActionName = $"{ActionName.Reload}->Hold";

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
                BotStore.Bots[botIdx].ActionState.WantSet.Add(ReleaseReloadDummyActionName);
                continue;
            }

            BotStore.Bots[botIdx].ActionState.WantSet.Add(HoldReleaseDummyActionName);
        }
    }
}

namespace ReBot.Main.AI.NeckSnap;

internal class NeckSnapSystem
{
    static readonly string SnapDummyActionName = $"{ActionName.Shoot}->Click";

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

            BotStore.Bots[botIdx].ActionState.WantSet.Add(SnapDummyActionName);
        }
    }
}

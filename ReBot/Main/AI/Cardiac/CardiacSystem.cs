namespace ReBot.Main.AI.Cardiac;

internal class CardiacSystem
{
    static readonly string CardiacDummyActionName = $"{ActionName.Shoot}->Click";

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

            BotStore.Bots[botIdx].ActionState.WantSet.Add(CardiacDummyActionName);
        }
    }
}

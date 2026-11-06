namespace ReBot.Main.AI.State.Cardiac;

internal class CardiacSystem
{
    static readonly string CardiacDummyActionName = $"{ActionName.Shoot}->Click";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            ref var cardiacState = ref BotStore.Bots[botIdx].CardiacState;

            var cardiacPositionRef = cardiacState.CardiacPositionRef;
            var (hasPosition, _) = cardiacPositionRef.Value;
            cardiacPositionRef.Value = default;

            var isHeld = cardiacState.IsHeld;
            var prevIsHeld = isHeld;

            if (hasPosition && !isHeld)
            {
                isHeld = true;
            }
            else
            {
                isHeld = false;
            }

            cardiacState.IsHeld = isHeld;

            if (prevIsHeld != isHeld)
            {
                if (isHeld)
                {
                    BotStore.Bots[botIdx].ActionState.WantSet.Add(CardiacDummyActionName);
                }
            }
        }
    }
}

namespace ReBot.Main.AI.State.NeckSnap;

internal class NeckSnapSystem
{
    static readonly string SnapDummyActionName = $"{ActionName.Shoot}->Click";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            ref var snapState = ref BotStore.Bots[botIdx].NeckSnapState;

            var snapPositionRef = snapState.SnapPositionRef;
            var (hasPosition, _) = snapPositionRef.Value;
            snapPositionRef.Value = default;

            var isHeld = snapState.IsHeld;
            var prevIsHeld = isHeld;

            if (hasPosition && !isHeld)
            {
                isHeld = true;
            }
            else
            {
                isHeld = false;
            }

            snapState.IsHeld = isHeld;

            if (prevIsHeld != isHeld)
            {
                if (isHeld)
                {
                    BotStore.Bots[botIdx].ActionState.WantSet.Add(SnapDummyActionName);
                }
            }
        }
    }
}

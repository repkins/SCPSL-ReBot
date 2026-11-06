namespace ReBot.Main.AI.State.Snap;

internal class SnapSystem
{
    static readonly string SnapDummyActionName = $"{ActionName.Shoot}->Click";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            ref var snapState = ref BotStore.Bots[botIdx].SnapStore;

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
                    BotStore.Bots[botIdx].ActionStore.WantSet.Add(SnapDummyActionName);
                }
            }
        }
    }
}

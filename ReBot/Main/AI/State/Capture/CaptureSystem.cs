namespace ReBot.Main.AI.State.Capture;

internal class CaptureSystem
{
    static readonly string CaptureDummyActionName = $"{ActionName.Shoot}->Click";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            ref var captureState = ref BotStore.Bots[botIdx].CaptureStore;

            var capturePositionRef = captureState.CapturePositionRef;
            var (hasPosition, _) = capturePositionRef.Value;
            capturePositionRef.Value = default;

            var isHeld = captureState.IsHeld;
            var prevIsHeld = isHeld;

            if (hasPosition && !isHeld)
            {
                isHeld = true;
            }
            else
            {
                isHeld = false;
            }

            captureState.IsHeld = isHeld;

            if (prevIsHeld != isHeld)
            {
                if (isHeld)
                {
                    BotStore.Bots[botIdx].ActionStore.WantSet.Add(CaptureDummyActionName);
                }
            }
        }
    }
}

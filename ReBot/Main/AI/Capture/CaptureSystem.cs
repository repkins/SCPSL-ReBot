namespace ReBot.Main.AI.Capture;

internal class CaptureSystem
{
    static readonly string CaptureDummyActionName = $"{ActionName.Shoot}->Click";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var capturePositionRef = BotStore.Bots[botIdx].CaptureState.CapturePositionRef;

            var (hasPosition, _) = capturePositionRef.Value;
            capturePositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            BotStore.Bots[botIdx].ActionState.WantSet.Add(CaptureDummyActionName);
        }
    }
}

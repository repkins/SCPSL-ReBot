using NetworkManagerUtils.Dummies;

namespace ReBot.Main.AI.Action;

internal static class DummyActionSystem
{
    public static void Procees()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var hub = BotStore.Bots[botIdx].Hub;
            var dummyActions = DummyActionCollector.ServerGetActions(hub);

            foreach (var dummyAction in dummyActions)
            {
                if (BotStore.Bots[botIdx].ActionState.WantSet.Contains(dummyAction.Name))
                {
                    dummyAction.Action.Invoke();
                }
            }

            BotStore.Bots[botIdx].ActionState.WantSet.Clear();
        }
    }
}

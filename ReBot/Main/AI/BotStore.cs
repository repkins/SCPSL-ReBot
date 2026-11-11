using ReBot.Main.AI.Decision;
using ReBot.Main.AI.State.Movement.Pathfinding;
using System.Collections.Generic;

namespace ReBot.Main.AI;

internal static class BotStore
{
    public readonly static List<Bot> Bots = [];
    public readonly static Dictionary<ReferenceHub, BotRef> BotHubRefs = [];

    public static void AddBotPlayer(ReferenceHub referenceHub)
    {
        var nextBotIdx = Bots.Count;

        Bots.Add(new(referenceHub));
        BotHubRefs.Add(referenceHub, new(nextBotIdx));

        PathfindingStore.AddBot(new(nextBotIdx));
        MindStore.AddBot(new(nextBotIdx));
    }

    public static void RemoveBotPlayer(BotRef botRef)
    {
        MindStore.RemoveBot(botRef);
        PathfindingStore.RemoveBot(botRef);

        var hub = Bots[botRef.Idx].Hub;
        BotHubRefs.Remove(hub);

        Bots[botRef.Idx].Dispose();
        Bots.RemoveAt(botRef.Idx);

        for (int botIdx = 0; botIdx < Bots.Count; botIdx++)
        {
            var botHub = Bots[botIdx].Hub;
            BotHubRefs[botHub] = new(botIdx);
        }
    }
}

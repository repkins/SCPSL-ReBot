using NetworkManagerUtils.Dummies;
using ReBot.Main.AI.Mind;
using ReBot.Main.AI.Movement.Pathfinding;
using System.Collections.Generic;

namespace ReBot.Main.AI;

public static class BotStore
{
    public readonly static List<Bot> Bots = [];
    public readonly static Dictionary<ReferenceHub, BotRef> BotHubRefs = [];

    public static void AddBotPlayer()
    {
        var nextBotIdx = Bots.Count;

        var referenceHub = InstantiatePlayerObject($"Bot{nextBotIdx}");
        Bots.Add(new(referenceHub));
        BotHubRefs.Add(referenceHub, new(nextBotIdx));

        PathfindingStore.AddBot(new(nextBotIdx));
        MindStore.AddBot(new(nextBotIdx));
    }

    public static void RemoveBotPlayer(BotRef botRef)
    {
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

    private static ReferenceHub InstantiatePlayerObject(string nickname)
    {
        var referenceHub = DummyUtils.SpawnDummy(nickname);

        return referenceHub;
    }
}

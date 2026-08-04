using Unity.Collections;

namespace ReBot.Main.AI.Mind;

internal class MindStore
{
    public static NativeList<BeliefRef> ActionBeliefRefs = new(Allocator.Persistent);

    public static void AddBot(BotRef botRef)
    {
        ActionBeliefRefs.Add(default);
    }

    public static void RemoveBot(BotRef botRef)
    {
        ActionBeliefRefs.RemoveAt(botRef.Idx);
    }
}

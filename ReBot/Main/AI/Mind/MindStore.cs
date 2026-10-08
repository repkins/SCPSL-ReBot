using Unity.Collections;

namespace ReBot.Main.AI.Mind;

internal class MindStore
{
    public static NativeList<ActionRef> EnabledActionRefs = new(Allocator.Persistent);

    public static void AddBot(BotRef botRef)
    {
        EnabledActionRefs.Add(default);
    }

    public static void RemoveBot(BotRef botRef)
    {
        EnabledActionRefs.RemoveAt(botRef.Idx);
    }
}

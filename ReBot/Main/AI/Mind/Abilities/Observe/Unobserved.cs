using ReBot.Main.AI.Mind.Abilities.MoveTo;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Observe;

internal struct Unobserved<TMoveTo>(BotRef botRef, TMoveTo moveTo)
    where TMoveTo : IMovedTo
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;

    public bool Impacting(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        if (!isObserved)
        {
            return moveTo.Impacting(targetLocationIdx, impactingActions);
        }

        return false;
    }
    public bool Enabling(int targetLocationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        return moveTo.Enabling(targetLocationIdx, beliefsToEnable);
    }
}

using ReBot.Main.AI.Decision;
using ReBot.Main.AI.Decision.Abilities.MoveTo;
using Unity.Collections;

namespace ReBot.Main.AI.Decision.Abilities.Observing;

internal struct Unobserved<TMoveTo>(BotRef botRef, TMoveTo moveTo)
    where TMoveTo : IMoveTo
{
    bool isObserved = botRef.Value.BlinkStore.IsObserved;

    public void Impacting(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        if (!isObserved)
        {
            moveTo.Impacting(targetLocationIdx, impactingActions);
        }
    }
    public void Enabling(int targetLocationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        moveTo.Enabling(targetLocationIdx, beliefsToEnable);
    }
}

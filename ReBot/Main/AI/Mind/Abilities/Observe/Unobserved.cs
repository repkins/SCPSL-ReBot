using ReBot.Main.AI.Mind.Abilities.MoveTo;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Observe;

internal struct Unobserved<TMoveToImpacting>(BotRef botRef, TMoveToImpacting moveToImpacting)
    where TMoveToImpacting : IMoveToImpacting
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier = 1f)
    {
        if (!isObserved)
        {
            return moveToImpacting.Process(targetLocationIdx, impactingActions, multiplier);
        }

        return false;
    }
}

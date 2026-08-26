using ReBot.Main.AI.Mind.Abilities.Move;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Unobserved<TMovableToTarget>(BotRef botRef, TMovableToTarget movableToTarget)
    where TMovableToTarget : IMovableToTarget
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;

    TMovableToTarget movableToTarget = movableToTarget;

    public bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (!isObserved)
        {
            return movableToTarget.GoesTo(targetLocationIdx, impactingBeliefs);
        }

        return false;
    }

    public bool MovesTo(int targetLocationIdx)
    {
        return movableToTarget.MovesTo(targetLocationIdx);
    }
}

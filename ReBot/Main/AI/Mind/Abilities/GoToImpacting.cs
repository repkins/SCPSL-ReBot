using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct GoToImpacting<TTarget>(BotRef botRef, TTarget target)
    where TTarget : ITarget
{
    MoveToImpacting<TTarget> moveToImpacting = new(botRef, target);

    public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs, float multiplier = 1f)
    {
        return moveToImpacting.Process(targetLocationIdx, impactingBeliefs, multiplier);
    }
}

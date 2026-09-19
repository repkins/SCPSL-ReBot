using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct GoTo<TTarget>
    where TTarget : ITarget
{
    internal struct Impacting(BotRef botRef, TTarget target)
    {
        MoveTo<TTarget>.Impacting moveToTargetImpacting = new(botRef, target);

        public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs, float multiplier = 1f)
        {
            return moveToTargetImpacting.Process(targetLocationIdx, impactingBeliefs, multiplier);
        }
    }
}

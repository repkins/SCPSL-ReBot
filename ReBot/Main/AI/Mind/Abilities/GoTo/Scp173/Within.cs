using ReBot.Main.AI.Mind.Abilities.Observe;
using ReBot.Main.AI.Mind.Abilities.BlinkTo;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.GoTo.Scp173;

internal struct Within<TTarget>(BotRef botRef, TTarget target) : IWithin
    where TTarget : ITarget
{
    Unobserved<MovedTo<TTarget>> unobservedMovedToTargetImpacting = new(botRef, new(botRef, target));
    ObservedBlinkedTo<TTarget> observedBlinkedToTargetImpacting = new(botRef, target);

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier = 1f)
    {
        return unobservedMovedToTargetImpacting.Process(targetLocationIdx, impactingActions, multiplier)
            || observedBlinkedToTargetImpacting.Process(targetLocationIdx, impactingActions, multiplier);
    }
}

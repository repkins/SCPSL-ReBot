using ReBot.Main.AI.Mind.Abilities.Observe;
using ReBot.Main.AI.Mind.Abilities.BlinkTo;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.GoTo.Scp173;

internal struct GoTo<TTarget>
    where TTarget : ITarget
{
    internal struct Impacting(BotRef botRef, TTarget target) : IGoToImpacting
    {
        Unobserved<MoveTo<TTarget>.Impacting> unobservedMoveToTargetImpacting = new(botRef, new(botRef, target));
        ObservedBlinkTo<TTarget>.Impacting observedBlinkToTargetImpacting = new(botRef, target);

        public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier = 1f)
        {
            return unobservedMoveToTargetImpacting.Process(targetLocationIdx, impactingActions, multiplier)
                || observedBlinkToTargetImpacting.Process(targetLocationIdx, impactingActions, multiplier);
        }
    }
}

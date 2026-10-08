using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.GoTo;

internal struct Within<TTarget>(BotRef botRef, TTarget target) : IWithin
    where TTarget : ITarget
{
    MovedTo<TTarget> movedToTarget = new(botRef, target);

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier = 1f)
    {
        return movedToTarget.Process(targetLocationIdx, impactingActions, multiplier);
    }
}

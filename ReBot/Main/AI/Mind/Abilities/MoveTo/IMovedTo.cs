using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal interface IMovedTo
{
    bool Impacting(int targetLocationIdx, NativeList<ImpactingAction> impactingActions);
    bool Enabling(int targetLocationIdx, NativeList<BeliefRef> beliefsToEnable);
}

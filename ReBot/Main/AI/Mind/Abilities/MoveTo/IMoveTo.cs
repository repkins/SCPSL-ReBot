using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal interface IMoveTo
{
    void Impacting(int targetLocationIdx, NativeList<ImpactingAction> impactingActions);
    void Enabling(int targetLocationIdx, NativeList<BeliefRef> beliefsToEnable);
}

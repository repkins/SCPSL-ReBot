using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal interface IMoveToImpacting
{
    bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier);
}

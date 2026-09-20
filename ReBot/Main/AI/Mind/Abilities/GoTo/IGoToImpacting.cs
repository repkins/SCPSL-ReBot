using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.GoTo;

internal interface IGoToImpacting
{
    bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs, float costMultiplier = 1f);
}

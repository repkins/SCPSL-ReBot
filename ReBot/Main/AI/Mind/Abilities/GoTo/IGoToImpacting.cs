using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.GoTo;

internal interface IGoToImpacting
{
    bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float costMultiplier = 1f);
}

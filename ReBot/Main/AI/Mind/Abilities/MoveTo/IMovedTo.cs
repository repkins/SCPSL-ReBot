using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal interface IMovedTo
{
    bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier);
}

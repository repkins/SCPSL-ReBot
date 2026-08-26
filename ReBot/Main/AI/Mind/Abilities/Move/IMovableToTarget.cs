using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal interface IMovableToTarget
{
    bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs);
    bool MovesTo(int targetLocationIdx);
}

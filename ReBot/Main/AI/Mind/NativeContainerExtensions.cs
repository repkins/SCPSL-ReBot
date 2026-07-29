using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public static class NativeContainerExtensions
{
    public static void Add(this NativeList<ImpactingBelief> impactingBeliefs, float cost, BeliefEnum beliefEnum, int beliefIdx)
    {
        impactingBeliefs.Add(new(cost, new(beliefEnum, beliefIdx)));
    }
}

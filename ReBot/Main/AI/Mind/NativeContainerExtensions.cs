using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public static class NativeContainerExtensions
{
    public static void Add(this NativeList<ImpactingBelief> impactingBeliefs, float cost, float multiplier, BeliefEnum beliefEnum, int beliefIdx)
    {
        impactingBeliefs.Add(new(cost, multiplier, new(beliefEnum, beliefIdx)));
    }

    public static void Add(this NativeList<ImpactingBelief> impactingBeliefs, float cost, BeliefEnum beliefEnum, int beliefIdx)
    {
        impactingBeliefs.Add(new(cost, 1f, new(beliefEnum, beliefIdx)));
    }
}

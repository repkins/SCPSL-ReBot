using Unity.Collections;

namespace ReBot.AI.Mind;

internal struct BeliefToImpact(BeliefEnum beliefEnum, int generalCostsInitialCapacity)
{
    public readonly BeliefEnum beliefEnum = beliefEnum;
    public NativeHashMap<BeliefRef, float> generalCosts = new(generalCostsInitialCapacity, Allocator.Temp);
}

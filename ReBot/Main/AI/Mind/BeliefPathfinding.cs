using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public struct BeliefPathfinding()
{
    private NativeList<(float, (BeliefEnum, BeliefRef))> beliefsToVisit = new(Allocator.Temp);
    private NativeHashMap<(BeliefEnum, BeliefRef), float> generalCosts = new(1, Allocator.Temp);

    public bool TryDequeueMin(out (BeliefEnum beliefEnum, BeliefRef beliefRef) best)
    {
        best = default;

        if (beliefsToVisit.Length > 0)
        {
            var smallest = float.MaxValue;
            int bestIdx = default;

            for (var i = 0; i < beliefsToVisit.Length; i++)
            {
                var (priority, belief) = beliefsToVisit[i];
                if (priority < smallest)
                {
                    smallest = priority;
                    best = belief;
                    bestIdx = i;
                }
            }

            beliefsToVisit.RemoveAtSwapBack(bestIdx);

            return true;
        }

        return false;
    }

    public void EnqueueIfLess((BeliefEnum, BeliefRef) beliefToEnable, float cost, (BeliefEnum, BeliefRef) impactingBelief)
    {
        var enablingBeliefCost = generalCosts[beliefToEnable];
        var generalCost = enablingBeliefCost + cost;

        if (!generalCosts.TryGetValue(impactingBelief, out var visitedCost)
            || visitedCost > generalCost)
        {
            generalCosts[impactingBelief] = generalCost;
            var totalCost = generalCost;

            beliefsToVisit.Add((totalCost, impactingBelief));
        }
    }
}

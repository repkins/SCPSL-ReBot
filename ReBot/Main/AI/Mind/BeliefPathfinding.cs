using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public struct BeliefPathfinding()
{
    private NativeList<(float, BeliefRef)> beliefsToVisit = new(Allocator.Temp);
    public NativeHashMap<BeliefRef, (float cost, float multiplier)> generalCosts = new(1, Allocator.Temp);
    
    public NativeParallelMultiHashMap<BeliefRef, BeliefRef> visitedImpactingBeliefs = new(1, Allocator.Temp);

    public bool TryDequeueMin(out BeliefRef best)
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

    public void EnqueueIfLess(BeliefRef beliefToEnable, float cost, BeliefRef impactingBelief)
    {
        EnqueueIfLess(beliefToEnable, cost, multiplier: 1f, impactingBelief);
    }

    public void EnqueueIfLess(BeliefRef beliefToEnable, float cost, float multiplier, BeliefRef impactingBelief)
    {
        var (enablingBeliefCost, enablingBeliefMultiplier) = generalCosts[beliefToEnable];
        multiplier *= enablingBeliefMultiplier;

        var generalCost = enablingBeliefCost + cost * multiplier;

        if (!generalCosts.TryGetValue(impactingBelief, out var visitedCostTuple)
            || visitedCostTuple.cost > generalCost)
        {
            EnqueueIfLess(generalCost, multiplier, impactingBelief);
            visitedImpactingBeliefs.Add(beliefToEnable, impactingBelief);
        }
    }

    public void EnqueueIfLess(float generalCost, BeliefRef impactingBelief)
    {
        EnqueueIfLess(generalCost, multiplier: 1f, impactingBelief);
    }

    public void EnqueueIfLess(float generalCost, float multiplier, BeliefRef impactingBelief)
    {
        generalCosts[impactingBelief] = (generalCost, multiplier);
        var totalCost = generalCost;

        beliefsToVisit.Add((totalCost, impactingBelief));
    }
}

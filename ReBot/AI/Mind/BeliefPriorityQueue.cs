using Unity.Collections;

namespace ReBot.AI.Mind;

internal struct BeliefPriorityQueue()
{
    private NativeList<(float, (BeliefEnum, BeliefRef))> list = new(Allocator.Temp);

    public bool TryDequeueMin(out BeliefEnum bestEnum, out BeliefRef bestRef)
    {
        bestEnum = default;
        bestRef = default;

        if (list.Length > 0)
        {
            var smallest = float.MaxValue;
            int bestIdx = default;

            for (var i = 0; i < list.Length; i++)
            {
                var (priority, belief) = list[i];
                if (priority < smallest)
                {
                    smallest = priority;
                    (bestEnum, bestRef) = belief;
                    bestIdx = i;
                }
            }

            list.RemoveAtSwapBack(bestIdx);

            return true;
        }

        return false;
    }

    public void EnqueueIfLess(float beliefCost, float cost, ref BeliefToImpact beliefToImpact, BeliefRef beliefRef)
    {
        var generalCost = beliefCost + cost;

        if (!beliefToImpact.generalCosts.TryGetValue(beliefRef, out var visitedCost)
            || visitedCost > generalCost)
        {
            beliefToImpact.generalCosts[beliefRef] = generalCost;
            var totalCost = generalCost;

            list.Add((totalCost, (beliefToImpact.beliefEnum, beliefRef)));
        }
    }
}

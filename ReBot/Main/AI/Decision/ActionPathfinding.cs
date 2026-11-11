using Unity.Collections;

namespace ReBot.Main.AI.Decision;

internal struct ActionPathfinding()
{
    private NativeList<(float, ActionRef)> actionsToVisit = new(Allocator.Temp);
    public NativeHashMap<ActionRef, (float cost, float multiplier)> generalCosts = new(1, Allocator.Temp);
    
    public NativeParallelMultiHashMap<ActionRef, ActionRef> visitedImpactingActions = new(1, Allocator.Temp);

    public bool TryDequeueMin(out ActionRef best)
    {
        best = default;

        if (actionsToVisit.Length > 0)
        {
            var smallest = float.MaxValue;
            int bestIdx = default;

            for (var i = 0; i < actionsToVisit.Length; i++)
            {
                var (priority, action) = actionsToVisit[i];
                if (priority < smallest)
                {
                    smallest = priority;
                    best = action;
                    bestIdx = i;
                }
            }

            actionsToVisit.RemoveAtSwapBack(bestIdx);

            return true;
        }

        return false;
    }

    public void EnqueueIfLess(ActionRef actionToEnable, float cost, ActionRef impactingAction)
    {
        EnqueueIfLess(actionToEnable, cost, multiplier: 1f, impactingAction);
    }

    public void EnqueueIfLess(ActionRef actionToEnable, float cost, float multiplier, ActionRef impactingAction)
    {
        var (enablingBeliefCost, enablingBeliefMultiplier) = generalCosts[actionToEnable];
        multiplier *= enablingBeliefMultiplier;

        var generalCost = enablingBeliefCost + cost * multiplier;

        if (!generalCosts.TryGetValue(impactingAction, out var visitedCostTuple)
            || visitedCostTuple.cost > generalCost)
        {
            EnqueueIfLess(generalCost, multiplier, impactingAction);
            visitedImpactingActions.Add(actionToEnable, impactingAction);
        }
    }

    public void EnqueueIfLess(float generalCost, ActionRef impactingAction)
    {
        EnqueueIfLess(generalCost, multiplier: 1f, impactingAction);
    }

    public void EnqueueIfLess(float generalCost, float multiplier, ActionRef impactingAction)
    {
        generalCosts[impactingAction] = (generalCost, multiplier);
        var totalCost = generalCost;

        actionsToVisit.Add((totalCost, impactingAction));
    }
}

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Mind;

internal struct FindActionJob<TMind, TMindFactory> : IJob
    where TMind : IMind
    where TMindFactory : IMind.IFactory<TMind>, new()
{
    public required BotRef botRef;

    public void Execute()
    {
        var beliefPathfinding = new BeliefPathfinding();

        var mindFactory = new TMindFactory();
        var mind = mindFactory.Create(botRef);

        var goalImpactingBeliefs = new NativeList<ImpactingBelief>(1, Allocator.Temp);
        mind.ProcessGoals(goalImpactingBeliefs);
        foreach (var (cost, multiplier, impactingBelief) in goalImpactingBeliefs)
        {
            beliefPathfinding.EnqueueIfLess(cost, multiplier, impactingBelief);
        }

        var impactingBeliefs = new NativeList<ImpactingBelief>(4, Allocator.Temp);
        var actionFound = false;
        var beliefToEnable = default(BeliefRef);
        while (!actionFound && beliefPathfinding.TryDequeueMin(out beliefToEnable))
        {
            actionFound = mind.ProcessBeliefToEnable(beliefToEnable, impactingBeliefs);

            foreach (var (cost, multiplier, impactingBelief) in impactingBeliefs)
            {
                beliefPathfinding.EnqueueIfLess(beliefToEnable, cost, multiplier, impactingBelief);
            }

            impactingBeliefs.Clear();
        }

        if (MindStore.ActionBeliefRefs[botRef.Idx] != beliefToEnable)
        {
            MindStore.ActionBeliefRefs[botRef.Idx] = beliefToEnable;

            PrintVisitedBeliefs(beliefPathfinding, goalImpactingBeliefs);
        }
    }

    private void PrintVisitedBeliefs(BeliefPathfinding beliefPathfinding, NativeList<ImpactingBelief> goalImpactingBeliefs)
    {
        var impactingBeliefStack = new NativeList<(NativeParallelMultiHashMapIterator<BeliefRef> iterator, BeliefRef beliefToEnable)>(Allocator.Temp);
        var actionBelief = MindStore.ActionBeliefRefs[botRef.Idx];

        foreach (var (_, _, goalImpactingBelief) in goalImpactingBeliefs)
        {
            var impactingBelief = goalImpactingBelief;
            var beliefToEnable = default(BeliefRef);
            var beliefToEnableGeneralCost = 0f;

            do
            {
                var prefix = new string(' ', 4 * impactingBeliefStack.Length);
                var (generalCost, multiplier) = beliefPathfinding.generalCosts[impactingBelief];
                var cost = generalCost - beliefToEnableGeneralCost;

                Debug.Log($"{prefix}{impactingBelief.Type}({impactingBelief.Idx}), {cost} ({multiplier}x){(impactingBelief == actionBelief ? " <--" : "")}");

                beliefToEnable = impactingBelief;
                if (beliefPathfinding.visitedImpactingBeliefs.TryGetFirstValue(beliefToEnable, out impactingBelief, out var impactingIterator))
                {
                    impactingBeliefStack.Add((impactingIterator, beliefToEnable));
                    (beliefToEnableGeneralCost, _) = beliefPathfinding.generalCosts[beliefToEnable];
                }
                else
                {
                    while (impactingBeliefStack.Length > 0
                        && impactingBeliefStack[^1] is var beliefToEnableFrame)
                    {
                        beliefToEnable = beliefToEnableFrame.beliefToEnable;
                        (beliefToEnableGeneralCost, _) = beliefPathfinding.generalCosts[beliefToEnable];

                        if (beliefPathfinding.visitedImpactingBeliefs.TryGetNextValue(out impactingBelief, ref beliefToEnableFrame.iterator))
                        {
                            impactingBeliefStack[^1] = beliefToEnableFrame;
                            break;
                        }
                        else
                        {
                            impactingBeliefStack.RemoveAt(impactingBeliefStack.Length - 1);
                        }
                    }

                }
            }
            while (impactingBeliefStack.Length > 0);
        }
    }
}

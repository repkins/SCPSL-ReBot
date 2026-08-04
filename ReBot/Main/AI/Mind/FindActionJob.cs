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
        foreach (var (cost, impactingBelief) in goalImpactingBeliefs)
        {
            beliefPathfinding.EnqueueIfLess(cost, impactingBelief);
        }

        var impactingBeliefs = new NativeList<ImpactingBelief>(4, Allocator.Temp);
        var actionFound = false;
        while (!actionFound && beliefPathfinding.TryDequeueMin(out var beliefToEnable))
        {
            actionFound = mind.ProcessBeliefToEnable(beliefToEnable, impactingBeliefs);

            foreach (var (cost, impactingBelief) in impactingBeliefs)
            {
                beliefPathfinding.EnqueueIfLess(beliefToEnable, cost, impactingBelief);
            }

            impactingBeliefs.Clear();
        }



        PrintVisitedBeliefs(beliefPathfinding, goalImpactingBeliefs);
    }

    private static void PrintVisitedBeliefs(BeliefPathfinding beliefPathfinding, NativeList<ImpactingBelief> goalImpactingBeliefs)
    {
        var impactingBeliefIteratorStack = new NativeList<NativeParallelMultiHashMapIterator<BeliefRef>>(Allocator.Temp);
        foreach (var (_, goalBeliefToEnable) in goalImpactingBeliefs)
        {
            var outImpactingBelief = goalBeliefToEnable;

            do
            {
                var prefix = new string(' ', 4 * impactingBeliefIteratorStack.Length);
                Debug.Log($"{prefix}{outImpactingBelief}");

                if (beliefPathfinding.visitedImpactingBeliefs.TryGetFirstValue(outImpactingBelief, out outImpactingBelief, out var beliefToEnableIterator))
                {
                    impactingBeliefIteratorStack.Add(beliefToEnableIterator);
                }
                else
                {
                    while (impactingBeliefIteratorStack.Length > 0
                        && impactingBeliefIteratorStack[^1] is var beliefToEnableIteratorNext)
                    {
                        if (beliefPathfinding.visitedImpactingBeliefs.TryGetNextValue(out outImpactingBelief, ref beliefToEnableIteratorNext))
                        {
                            impactingBeliefIteratorStack[^1] = beliefToEnableIteratorNext;
                            break;
                        }
                        else
                        {
                            impactingBeliefIteratorStack.RemoveAt(impactingBeliefIteratorStack.Length - 1);
                        }
                    }

                }
            }
            while (impactingBeliefIteratorStack.Length > 0);
        }
    }
}

using Unity.Collections;
using Unity.Jobs;

namespace ReBot.Main.AI.Mind;

internal struct FindActionJob<TMind> : IJob
    where TMind : IMind
{
    public required TMind mind;

    public void Execute()
    {
        var beliefPathfinding = new BeliefPathfinding();

        var actionFound = false;
        var impactingBeliefs = new NativeList<(float cost, (BeliefEnum beliefEnum, BeliefRef beliefRef))>(4, Allocator.Temp);

        while (!actionFound && beliefPathfinding.TryDequeueMin(out var beliefToEnable))
        {
            actionFound = mind.ProcessBeliefToEnable(beliefToEnable, impactingBeliefs);

            foreach (var (cost, impactingBelief) in impactingBeliefs)
            {
                beliefPathfinding.EnqueueIfLess(beliefToEnable, cost, impactingBelief);
            }
        }
    }
}

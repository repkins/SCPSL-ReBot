using Unity.Collections;
using Unity.Jobs;

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

        var impactingBeliefs = new NativeList<ImpactingBelief>(4, Allocator.Temp);

        mind.ProcessGoals(impactingBeliefs);

        foreach (var (cost, impactingBelief) in impactingBeliefs)
        {
            beliefPathfinding.EnqueueIfLess(cost, impactingBelief);
        }

        impactingBeliefs.Clear();

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
    }
}

using ReBot.AI.Movement;
using ReBot.AI.Termination;
using ReBot.AI.Traversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;

namespace ReBot.AI.Mind;

internal class DecisionMakingSystem
{
    public static void Process()
    {
        var botCount = BotStore.BotHubs.Count;

        var findActionHandles = new NativeArray<JobHandle>(botCount, Allocator.TempJob);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var targets = TraversalState.Targets[botIdx];

            var findActionJob = new FindActionJob
            {
                targetPositions = targets,
                moveToPositionRef = MovementStore.MoveToPositionRefs[botIdx]
            };

            findActionHandles[botIdx] = findActionJob.Schedule();
        }

        var decisionMakingHandle = JobHandle.CombineDependencies(findActionHandles);
        decisionMakingHandle.Complete();

        findActionHandles.Dispose();
    }
}

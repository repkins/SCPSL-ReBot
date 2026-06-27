using ReBot.AI.Traversal;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Mind;

internal struct FindActionJob : IJob
{
    [ReadOnly]
    public required Vector3 playerPosition;

    [ReadOnly]
    public required NativeList<Vector3> targetPositions;
    [ReadOnly]
    public required NativeList<(bool, TargetRef)> obstacleTargets;

    [WriteOnly]
    public required NativeReference<(bool HasPosition, Vector3 Position)> moveToPositionRef;
    [WriteOnly]
    public required NativeReference<(bool HasPosition, Vector3 Position)> interactPositionRef;

    public void Execute()
    {
        var beliefsToVisit = new BeliefPriorityQueue();

        var moveToTarget = new BeliefToImpact(BeliefEnum.MoveToTarget, targetPositions.Length);
        var obstacleTarget = new BeliefToImpact(BeliefEnum.ObstacleTarget, obstacleTargets.Length);

        while (!moveToPositionRef.Value.HasPosition && beliefsToVisit.TryDequeueMin(out var beliefEnum, out var beliefRef))
        {
            switch (beliefEnum) 
            {
                case BeliefEnum.MoveToTarget:
                    {
                        var targetPosition = targetPositions[beliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);
                    }
                    break;

                case BeliefEnum.InteractWithTarget:
                    {
                        var targetPosition = targetPositions[beliefRef.Idx];
                        interactPositionRef.Value = (true, targetPosition);
                    }
                    break;

                case BeliefEnum.ObstacleTarget:
                    {
                        var beliefCost = obstacleTarget.generalCosts[beliefRef];

                        var (hasObstacle2, obstacle2TargetRef) = obstacleTargets[beliefRef.Idx];
                        if (!hasObstacle2)
                        {
                            var cost = Vector3.Distance(targetPositions[beliefRef.Idx], playerPosition);

                            beliefsToVisit.EnqueueIfLess(beliefCost, cost, ref moveToTarget, new(beliefRef.Idx));
                        }
                        else
                        {
                            var cost = Vector3.Distance(targetPositions[beliefRef.Idx], targetPositions[obstacle2TargetRef.Idx]);

                            beliefsToVisit.EnqueueIfLess(beliefCost, cost, ref obstacleTarget, new(obstacle2TargetRef.Idx));
                        }
                    }
                    break;
            }
        }
    }
}

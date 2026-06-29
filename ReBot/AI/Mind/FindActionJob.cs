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
        var beliefPathfinding = new BeliefPathfinding();

        while (!moveToPositionRef.Value.HasPosition && beliefPathfinding.TryDequeueMin(out var enablingBeliefEnum, out var enablingBeliefRef))
        {
            switch (enablingBeliefEnum) 
            {
                case BeliefEnum.WithinTarget:
                    {
                        var targetPosition = targetPositions[enablingBeliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);
                    }
                    break;

                case BeliefEnum.InteractingWithTarget:
                    {
                        var targetPosition = targetPositions[enablingBeliefRef.Idx];
                        interactPositionRef.Value = (true, targetPosition);
                    }
                    break;

                case BeliefEnum.NoObstacleTarget:
                    {
                        var (hasObstacle2, obstacle2TargetRef) = obstacleTargets[enablingBeliefRef.Idx];
                        if (!hasObstacle2)
                        {
                            var cost = Vector3.Distance(targetPositions[enablingBeliefRef.Idx], playerPosition);

                            beliefPathfinding.EnqueueIfLess((enablingBeliefEnum, enablingBeliefRef), cost, (BeliefEnum.WithinTarget, enablingBeliefRef));
                        }
                        else
                        {
                            var cost = Vector3.Distance(targetPositions[enablingBeliefRef.Idx], targetPositions[obstacle2TargetRef.Idx]);

                            beliefPathfinding.EnqueueIfLess((enablingBeliefEnum, enablingBeliefRef), cost, (BeliefEnum.NoObstacleTarget, new(obstacle2TargetRef.Idx)));
                        }
                    }
                    break;
            }
        }
    }
}

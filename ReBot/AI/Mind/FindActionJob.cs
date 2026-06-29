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

    [ReadOnly]
    public required NativeList<TargetRef> roomEnterLocationTargets;

    [ReadOnly]
    public required NativeList<TargetRef> sightedEnemyTargets;
    [ReadOnly]
    public required NativeList<bool> enemiesWithinDistance;

    [WriteOnly]
    public required NativeReference<(bool HasPosition, Vector3 Position)> moveToPositionRef;
    [WriteOnly]
    public required NativeReference<(bool HasPosition, Vector3 Position)> interactPositionRef;

    [WriteOnly]
    public required NativeReference<(bool HasPosition, Vector3 Position)> cardiacPositionRef;

    public void Execute()
    {
        var beliefPathfinding = new BeliefPathfinding();

        var actionFound = false;

        while (!actionFound && beliefPathfinding.TryDequeueMin(out var enablingBelief))
        {
            switch (enablingBelief.beliefEnum) 
            {
                case BeliefEnum.TargetsTerminated:
                    {
                        if (sightedEnemyTargets.Length > 0)
                        {
                            for (var enemyIdx = 0; enemyIdx < sightedEnemyTargets.Length; enemyIdx++)
                            {
                                var enemyTargetRef = sightedEnemyTargets[enemyIdx];

                                if (enemiesWithinDistance[enemyIdx])
                                {
                                    var cost = 1;

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.CardiacTarget, new(enemyTargetRef.Idx)));
                                }
                                else
                                {
                                    var cost = 1;

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.EnemyWithinDistance, new(enemyTargetRef.Idx)));
                                }
                            }
                        }
                        else
                        {
                            var cost = 30; // max sight distance

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.SightedEnemies, default));
                        }
                    }
                    break;

                case BeliefEnum.EnemyWithinDistance:
                    {
                        var enemyRef = enablingBelief.beliefRef;
                        var enemyTargetRef = sightedEnemyTargets[enemyRef.Idx];

                        var (hasObstacle, obstacleTargetRef) = obstacleTargets[enemyTargetRef.Idx];
                        if (!hasObstacle)
                        {
                            var cost = Vector3.Distance(targetPositions[enemyTargetRef.Idx], playerPosition);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinTarget, new(enemyTargetRef.Idx)));
                        }
                        else
                        {
                            var cost = Vector3.Distance(targetPositions[enemyTargetRef.Idx], targetPositions[obstacleTargetRef.Idx]);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleTargetToRemove, new(obstacleTargetRef.Idx)));
                        }
                    }
                    break;

                case BeliefEnum.SightedEnemies:
                    {
                        if (roomEnterLocationTargets.Length > 0)
                        {
                            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < roomEnterLocationTargets.Length; roomEnterLocationIdx++)
                            {
                                var roomEnterLocationTargetRef = roomEnterLocationTargets[roomEnterLocationIdx];

                                var (hasObstacle, obstacleTargetRef) = obstacleTargets[roomEnterLocationTargetRef.Idx];
                                if (!hasObstacle)
                                {
                                    var cost = Vector3.Distance(targetPositions[roomEnterLocationTargetRef.Idx], playerPosition);
                                    cost *= 10; // multiplier is odds of seeing enemies

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinTarget, new(roomEnterLocationTargetRef.Idx)));
                                }
                                else
                                {
                                    var cost = Vector3.Distance(targetPositions[roomEnterLocationTargetRef.Idx], targetPositions[obstacleTargetRef.Idx]);
                                    cost *= 10; // multiplier is odds of seeing enemies

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleTargetToRemove, new(obstacleTargetRef.Idx)));
                                }
                            }
                        }
                    }
                    break;

                case BeliefEnum.ObstacleTargetToRemove:
                    {
                        var (hasObstacle2, obstacle2TargetRef) = obstacleTargets[enablingBelief.beliefRef.Idx];
                        if (!hasObstacle2)
                        {
                            var cost = Vector3.Distance(targetPositions[enablingBelief.beliefRef.Idx], playerPosition);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinTarget, enablingBelief.beliefRef));
                        }
                        else
                        {
                            var cost = Vector3.Distance(targetPositions[enablingBelief.beliefRef.Idx], targetPositions[obstacle2TargetRef.Idx]);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleTargetToRemove, new(obstacle2TargetRef.Idx)));
                        }
                    }
                    break;

                case BeliefEnum.WithinTarget:
                    {
                        var targetPosition = targetPositions[enablingBelief.beliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.InteractingWithTarget:
                    {
                        var targetPosition = targetPositions[enablingBelief.beliefRef.Idx];
                        interactPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.CardiacTarget:
                    {
                        var targetPosition = targetPositions[enablingBelief.beliefRef.Idx];
                        cardiacPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;
            }
        }
    }
}

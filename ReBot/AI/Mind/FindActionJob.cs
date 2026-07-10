using ReBot.AI.Spacial;
using ReBot.AI.Termination;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Mind;

internal struct FindActionJob : IJob
{
    [ReadOnly]
    public required Vector3 playerPosition;

    [ReadOnly]
    public required NativeHashMap<Vector3, ObstacleRef> obstacles;
    [ReadOnly]
    public required NativeList<Vector3> obstacleLocations;

    [ReadOnly]
    public required NativeList<Vector3> roomEnterLocations;

    [ReadOnly]
    public required NativeList<Vector3> enemiesWithinSight;
    [ReadOnly]
    public required NativeList<EnemyRef> enemiesWithinDistance;

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
                        if (enemiesWithinSight.Length > 0)
                        {
                            for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
                            {
                                var cost = 1f;  // cardiac attack cost, constant

                                if (enemiesWithinDistance.Contains(new EnemyRef(enemyIdx)))
                                {
                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.CardiacEnemy, new(enemyIdx)));
                                }
                                else
                                {
                                    var enemyPosition = enemiesWithinSight[enemyIdx];

                                    if (!obstacles.TryGetValue(enemyPosition, out var obstacleRef))
                                    {
                                        cost += Vector3.Distance(enemyPosition, playerPosition);

                                        beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinEnemyLocation, new(enemyIdx)));
                                    }
                                    else
                                    {
                                        cost += Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                                        beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToRemove, new(obstacleRef.Idx)));
                                    }
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

                case BeliefEnum.SightedEnemies:
                    {
                        if (roomEnterLocations.Length > 0)
                        {
                            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < roomEnterLocations.Length; roomEnterLocationIdx++)
                            {
                                var roomEnterLocationPosition = roomEnterLocations[roomEnterLocationIdx];

                                var cost = 10f; // multiplier is odds of seeing enemies

                                if (!obstacles.TryGetValue(roomEnterLocationPosition, out var obstacleRef))
                                {
                                    cost *= Vector3.Distance(roomEnterLocationPosition, playerPosition);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinRoomEnterLocation, new(roomEnterLocationIdx)));
                                }
                                else
                                {
                                    cost *= Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToRemove, new(obstacleRef.Idx)));
                                }
                            }
                        }
                    }
                    break;

                case BeliefEnum.ObstacleToRemove:
                    {
                        var obstaclePosition = obstacleLocations[enablingBelief.beliefRef.Idx];

                        if (!obstacles.TryGetValue(obstaclePosition, out var obstacle2Ref))
                        {
                            var cost = Vector3.Distance(obstaclePosition, playerPosition);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinObstacleLocation, enablingBelief.beliefRef));
                        }
                        else
                        {
                            var cost = Vector3.Distance(obstaclePosition, obstacleLocations[obstacle2Ref.Idx]);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToRemove, new(obstacle2Ref.Idx)));
                        }
                    }
                    break;

                case BeliefEnum.WithinEnemyLocation:
                    {
                        var targetPosition = enemiesWithinSight[enablingBelief.beliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.WithinObstacleLocation:
                    {
                        var targetPosition = obstacleLocations[enablingBelief.beliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.WithinRoomEnterLocation:
                    {
                        var targetPosition = roomEnterLocations[enablingBelief.beliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.InteractingWithObstacle:
                    {
                        var targetPosition = obstacleLocations[enablingBelief.beliefRef.Idx];
                        interactPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.CardiacEnemy:
                    {
                        var targetPosition = enemiesWithinSight[enablingBelief.beliefRef.Idx];
                        cardiacPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;
            }
        }
    }
}

using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Mind;

internal struct FindActionJob : IJob
{
    [ReadOnly]
    public required Vector3 playerPosition;

    [ReadOnly] public required NativeList<Vector3> obstacleLocations;

    [ReadOnly] public required NativeList<ObstacleRef> elevationTravelObstacles;
    [ReadOnly] public required NativeList<Vector3> elevationTravelEnterLocations;
    [ReadOnly] public required NativeList<Vector3> elevationTravelInteractLocations;

    [ReadOnly] public required NativeList<ObstacleRef> elevationCallObstacles;
    [ReadOnly] public required NativeList<Vector3> elevationCallInteractLocations;

    [ReadOnly]
    public required NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget;

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

                                    if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
                                    {
                                        cost += Vector3.Distance(enemyPosition, playerPosition);

                                        beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinEnemyLocation, new(enemyIdx)));
                                    }
                                    else
                                    {
                                        cost += Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                                        beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx)));
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

                                if (!obstaclesByTarget.TryGetValue(roomEnterLocationPosition, out var obstacleRef))
                                {
                                    cost *= Vector3.Distance(roomEnterLocationPosition, playerPosition);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinRoomEnterLocation, new(roomEnterLocationIdx)));
                                }
                                else
                                {
                                    cost *= Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx)));
                                }
                            }
                        }
                    }
                    break;

                case BeliefEnum.ObstacleToOvercome:
                    {
                        var beliefObstacleRef = enablingBelief.beliefRef;
                        var obstaclePosition = obstacleLocations[beliefObstacleRef.Idx];

                        if (elevationTravelObstacles.IndexOf(new ObstacleRef(enablingBelief.beliefRef.Idx)) is var elevationIdx && elevationIdx > -1)
                        {
                            var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

                            var cost = 5f;

                            if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
                            {
                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.TravelingOnElevator, new(elevationIdx)));
                            }
                            else
                            {
                                if (!obstaclesByTarget.TryGetValue(elevationEnterPosition, out var obstacle2Ref))
                                {
                                    cost += Vector3.Distance(elevationEnterPosition, playerPosition);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.InElevator, new(elevationIdx)));
                                }
                                else
                                {
                                    cost += Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx)));
                                }
                            }
                        }
                        else if (elevationCallObstacles.IndexOf(new ObstacleRef(enablingBelief.beliefRef.Idx)) is var elevationCallIdx && elevationCallIdx > -1)
                        {
                            var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

                            var cost = Vector3.Distance(obstaclePosition, elevationCallInteractPosition) + 5f;

                            if (!obstaclesByTarget.TryGetValue(elevationCallInteractPosition, out var obstacle2Ref))
                            {
                                cost += Vector3.Distance(elevationCallInteractPosition, playerPosition);

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ElevatorCalled, new(elevationCallIdx)));
                            }
                            else
                            {
                                cost += Vector3.Distance(elevationCallInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx)));
                            }
                        }
                        else
                        {
                            var obstacleInteractPosition = obstacleLocations[beliefObstacleRef.Idx];

                            var cost = Vector3.Distance(obstaclePosition, obstacleInteractPosition);

                            if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
                            {
                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.InteractingWithObstacle, beliefObstacleRef));
                            }
                            else
                            {
                                if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
                                {
                                    cost += Vector3.Distance(obstacleInteractPosition, playerPosition);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinObstacleLocation, beliefObstacleRef));
                                }
                                else
                                {
                                    cost += Vector3.Distance(obstacleInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                                    beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx)));
                                }
                            }
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

                case BeliefEnum.TravelingOnElevator:
                    {
                        var elevationIdx = enablingBelief.beliefRef.Idx;
                        var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
                        interactPositionRef.Value = (true, travelInteractPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.InElevator:
                    {
                        var elevationIdx = enablingBelief.beliefRef.Idx;
                        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
                        moveToPositionRef.Value = (true, elevationEnterPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.ElevatorCalled:
                    {
                        var elevationCallIdx = enablingBelief.beliefRef.Idx;
                        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

                        if (Vector3.Distance(elevationCallInteractPosition, playerPosition) < 2f)
                        {
                            interactPositionRef.Value = (true, elevationCallInteractPosition);
                        }
                        else
                        {
                            moveToPositionRef.Value = (true, elevationCallInteractPosition);
                        }

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

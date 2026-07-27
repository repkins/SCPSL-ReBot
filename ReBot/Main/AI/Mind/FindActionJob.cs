using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Mind;

internal struct FindActionJob : IJob
{
    public required Vector3 playerPosition;

    [ReadOnly] public required NativeList<Vector3> obstacleLocations;
    [ReadOnly] public required NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget;

    [ReadOnly] public required NativeList<ObstacleRef> elevationTravelObstacles;
    [ReadOnly] public required NativeList<Vector3> elevationTravelEnterLocations;
    [ReadOnly] public required NativeList<Vector3> elevationTravelInteractLocations;

    [ReadOnly] public required NativeList<ObstacleRef> elevationCallObstacles;
    [ReadOnly] public required NativeList<Vector3> elevationCallInteractLocations;

    [ReadOnly] public required NativeList<Vector3> roomEnterLocations;

    [ReadOnly] public required NativeList<Vector3> enemiesWithinSight;
    [ReadOnly] public required NativeList<EnemyRef> enemiesWithinDistance;

    [WriteOnly] public required NativeReference<(bool HasPosition, Vector3 Position)> moveToPositionRef;
    [WriteOnly] public required NativeReference<(bool HasPosition, Vector3 Position)> interactPositionRef;

    [WriteOnly] public required NativeReference<(bool HasPosition, Vector3 Position)> cardiacPositionRef;

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

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.CardiacEnemy, new(enemyIdx)));
                            }
                        }
                        else
                        {
                            var cost = 30; // max sight distance

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.SightedEnemies, default));
                        }
                    }
                    break;

                case BeliefEnum.CardiacEnemy:
                    {
                        var enemyIdx = enablingBelief.beliefRef.Idx;
                        var enemyPosition = enemiesWithinSight[enemyIdx];

                        if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
                        {
                            if (enemiesWithinDistance.Contains(new EnemyRef(enemyIdx)))
                            {
                                cardiacPositionRef.Value = (true, enemyPosition);

                                actionFound = true;
                            }
                            else
                            {
                                var cost = Vector3.Distance(enemyPosition, playerPosition);

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinEnemyLocation, new(enemyIdx)));
                            }
                        }
                        else
                        {
                            var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx)));
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

                case BeliefEnum.WithinEnemyLocation:
                    {
                        var targetPosition = enemiesWithinSight[enablingBelief.beliefRef.Idx];
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

                case BeliefEnum.ObstacleToOvercome:
                    {
                        var beliefObstacleRef = enablingBelief.beliefRef;
                        var obstaclePosition = obstacleLocations[beliefObstacleRef.Idx];

                        if (elevationTravelObstacles.IndexOf(new ObstacleRef(enablingBelief.beliefRef.Idx)) is var elevationIdx && elevationIdx > -1)
                        {
                            var cost = 5f;

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.TravelingOnElevator, new(elevationIdx)));
                        }
                        else if (elevationCallObstacles.IndexOf(new ObstacleRef(enablingBelief.beliefRef.Idx)) is var elevationCallIdx && elevationCallIdx > -1)
                        {
                            var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

                            var cost = Vector3.Distance(obstaclePosition, elevationCallInteractPosition) + 5f;

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ElevatorCalled, new(elevationCallIdx)));
                        }
                        else
                        {
                            var obstacleInteractPosition = obstacleLocations[beliefObstacleRef.Idx];

                            var cost = Vector3.Distance(obstaclePosition, obstacleInteractPosition) + 1f;

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.InteractingWithObstacle, beliefObstacleRef));
                        }
                    }
                    break;

                case BeliefEnum.InteractingWithObstacle:
                    {
                        var obstacleInteractIdx = enablingBelief.beliefRef.Idx;
                        var obstacleInteractPosition = obstacleLocations[obstacleInteractIdx];

                        if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
                        {
                            if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
                            {
                                interactPositionRef.Value = (true, obstacleInteractPosition);

                                actionFound = true;
                            }
                            else
                            {
                                var cost = Vector3.Distance(obstacleInteractPosition, playerPosition);

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.WithinObstacleInteractionLocation, new(obstacleInteractIdx)));
                            }
                        }
                        else
                        {
                            var cost = Vector3.Distance(obstacleInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx)));
                        }

                    }
                    break;

                case BeliefEnum.WithinObstacleInteractionLocation:
                    {
                        var targetPosition = obstacleLocations[enablingBelief.beliefRef.Idx];
                        moveToPositionRef.Value = (true, targetPosition);

                        actionFound = true;
                    }
                    break;

                case BeliefEnum.TravelingOnElevator:
                    {
                        var elevationIdx = enablingBelief.beliefRef.Idx;
                        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

                        if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
                        {
                            var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
                            interactPositionRef.Value = (true, travelInteractPosition);

                            actionFound = true;
                        }
                        else
                        {
                            if (!obstaclesByTarget.TryGetValue(elevationEnterPosition, out var obstacle2Ref))
                            {
                                var cost = Vector3.Distance(elevationEnterPosition, playerPosition);

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.InElevator, new(elevationIdx)));
                            }
                            else
                            {
                                var cost = Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                                beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx)));
                            }
                        }
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

                        if (!obstaclesByTarget.TryGetValue(elevationCallInteractPosition, out var obstacle2Ref))
                        {
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
                        else
                        {
                            var cost = Vector3.Distance(elevationCallInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                            beliefPathfinding.EnqueueIfLess(enablingBelief, cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx)));
                        }
                    }
                    break;
            }
        }
    }
}

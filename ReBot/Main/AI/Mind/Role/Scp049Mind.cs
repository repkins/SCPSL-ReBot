using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Role;

public struct Scp049Mind(BotRef botRef) : IMind
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<ObstacleRef> elevationTravelObstacles = ObstacleState.ElevationTravelObstacles;
    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
    NativeList<Vector3> elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations;
    NativeList<ObstacleRef> elevationCallObstacles = ObstacleState.ElevationCallObstacles;
    NativeList<Vector3> elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;

    NativeReference<(bool, Vector3)> cardiacPositionRef = botRef.Value.CardiacState.CardiacPositionRef;

    public bool ProcessBeliefToEnable((BeliefEnum beliefEnum, BeliefRef beliefRef) beliefToEnable, NativeList<(float cost, (BeliefEnum beliefEnum, BeliefRef beliefRef))> enablingBeliefs)
    {
        var actionFound = false;

        switch (beliefToEnable.beliefEnum)
        {
            case BeliefEnum.TargetsTerminated:
                {
                    if (enemiesWithinSight.Length > 0)
                    {
                        for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
                        {
                            var cost = 1f;  // cardiac attack cost, constant

                            enablingBeliefs.Add((cost, (BeliefEnum.CardiacEnemy, new(enemyIdx))));
                        }
                    }
                    else
                    {
                        var cost = 30; // max sight distance

                        enablingBeliefs.Add((cost, (BeliefEnum.SightedEnemies, default)));
                    }
                }
                break;

            case BeliefEnum.CardiacEnemy:
                {
                    var enemyIdx = beliefToEnable.beliefRef.Idx;
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

                            enablingBeliefs.Add((cost, (BeliefEnum.WithinEnemyLocation, new(enemyIdx))));
                        }
                    }
                    else
                    {
                        var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                        enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx))));
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

                                enablingBeliefs.Add((cost, (BeliefEnum.WithinRoomEnterLocation, new(roomEnterLocationIdx))));
                            }
                            else
                            {
                                cost *= Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                                enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx))));
                            }
                        }
                    }
                }
                break;

            case BeliefEnum.WithinEnemyLocation:
                {
                    var targetPosition = enemiesWithinSight[beliefToEnable.beliefRef.Idx];
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.WithinRoomEnterLocation:
                {
                    var targetPosition = roomEnterLocations[beliefToEnable.beliefRef.Idx];
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.ObstacleToOvercome:
                {
                    var beliefObstacleRef = beliefToEnable.beliefRef;
                    var obstaclePosition = obstacleLocations[beliefObstacleRef.Idx];

                    if (elevationTravelObstacles.IndexOf(new ObstacleRef(beliefToEnable.beliefRef.Idx)) is var elevationIdx && elevationIdx > -1)
                    {
                        var cost = 5f;

                        enablingBeliefs.Add((cost, (BeliefEnum.TravelingOnElevator, new(elevationIdx))));
                    }
                    else if (elevationCallObstacles.IndexOf(new ObstacleRef(beliefToEnable.beliefRef.Idx)) is var elevationCallIdx && elevationCallIdx > -1)
                    {
                        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

                        var cost = Vector3.Distance(obstaclePosition, elevationCallInteractPosition) + 5f;

                        enablingBeliefs.Add((cost, (BeliefEnum.ElevatorCalled, new(elevationCallIdx))));
                    }
                    else
                    {
                        var obstacleInteractPosition = obstacleLocations[beliefObstacleRef.Idx];

                        var cost = Vector3.Distance(obstaclePosition, obstacleInteractPosition) + 1f;

                        enablingBeliefs.Add((cost, (BeliefEnum.InteractingWithObstacle, beliefObstacleRef)));
                    }
                }
                break;

            case BeliefEnum.InteractingWithObstacle:
                {
                    var obstacleInteractIdx = beliefToEnable.beliefRef.Idx;
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

                            enablingBeliefs.Add((cost, (BeliefEnum.WithinObstacleInteractionLocation, new(obstacleInteractIdx))));
                        }
                    }
                    else
                    {
                        var cost = Vector3.Distance(obstacleInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                        enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx))));
                    }

                }
                break;

            case BeliefEnum.WithinObstacleInteractionLocation:
                {
                    var targetPosition = obstacleLocations[beliefToEnable.beliefRef.Idx];
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.TravelingOnElevator:
                {
                    var elevationIdx = beliefToEnable.beliefRef.Idx;
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

                            enablingBeliefs.Add((cost, (BeliefEnum.InElevator, new(elevationIdx))));
                        }
                        else
                        {
                            var cost = Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                            enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx))));
                        }
                    }
                }
                break;

            case BeliefEnum.InElevator:
                {
                    var elevationIdx = beliefToEnable.beliefRef.Idx;
                    var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
                    moveToPositionRef.Value = (true, elevationEnterPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.ElevatorCalled:
                {
                    var elevationCallIdx = beliefToEnable.beliefRef.Idx;
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

                        enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx))));
                    }
                }
                break;
        }

        return actionFound;
    }

    public struct Factory : IMind.IFactory<Scp049Mind>
    {
        public readonly Scp049Mind Create(BotRef botRef)
        {
            return new Scp049Mind(botRef);
        }
    }
}

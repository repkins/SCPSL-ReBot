using ReBot.Main.AI.Mind.Abilities;
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

    InteractsWithObstacle interactsWithObstacle = new(botRef);
    OvercomesInteractableObstacle overcomesInteractableObstacle = new();
    OvercomesElevationTravelObstacle overcomesElevationTravelObstacle = new();
    OvercomesElevationCallObstacle overcomesElevationCallObstacle = new();
    MovesToObstacleInteractLocation movesToObstacleInteractLocation = new(botRef);

    public bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var actionFound = false;

        switch (beliefToEnable.Type)
        {
            case BeliefEnum.TargetsTerminated:
                {
                    if (enemiesWithinSight.Length > 0)
                    {
                        for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
                        {
                            var cost = 1f;  // cardiac attack cost, constant

                            impactingBeliefs.Add(cost, BeliefEnum.CardiacEnemy, enemyIdx);
                        }
                    }
                    else
                    {
                        var cost = 30; // max sight distance

                        impactingBeliefs.Add(cost, BeliefEnum.SightedEnemies, default);
                    }
                }
                break;

            case BeliefEnum.CardiacEnemy:
                {
                    var enemyIdx = beliefToEnable.Idx;
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

                            impactingBeliefs.Add(cost, BeliefEnum.WithinEnemyLocation, enemyIdx);
                        }
                    }
                    else
                    {
                        var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                        impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
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

                                impactingBeliefs.Add(cost, BeliefEnum.WithinRoomEnterLocation, roomEnterLocationIdx);
                            }
                            else
                            {
                                cost *= Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                                impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
                            }
                        }
                    }
                }
                break;

            case BeliefEnum.WithinEnemyLocation:
                {
                    var targetPosition = enemiesWithinSight[beliefToEnable.Idx];
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.WithinRoomEnterLocation:
                {
                    var targetPosition = roomEnterLocations[beliefToEnable.Idx];
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.ObstacleToOvercome:
                {
                    if (elevationTravelObstacles.IndexOf(new ObstacleRef(beliefToEnable.Idx)) is var elevationIdx && elevationIdx > -1)
                    {
                        overcomesElevationTravelObstacle.Process(elevationIdx, impactingBeliefs);
                    }
                    else if (elevationCallObstacles.IndexOf(new ObstacleRef(beliefToEnable.Idx)) is var elevationCallIdx && elevationCallIdx > -1)
                    {
                        overcomesElevationCallObstacle.Process(new(beliefToEnable.Idx), elevationCallIdx, impactingBeliefs);
                    }
                    else
                    {
                        overcomesInteractableObstacle.Process(new(beliefToEnable.Idx), impactingBeliefs);
                    }
                }
                break;

            case BeliefEnum.InteractingWithObstacle:
                {
                    actionFound = interactsWithObstacle.Process(beliefToEnable.Idx, impactingBeliefs);
                }
                break;

            case BeliefEnum.WithinObstacleInteractionLocation:
                {
                    actionFound = movesToObstacleInteractLocation.Process(beliefToEnable.Idx);
                }
                break;

            case BeliefEnum.TravelingOnElevator:
                {
                    var elevationIdx = beliefToEnable.Idx;
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

                            impactingBeliefs.Add(cost, BeliefEnum.InElevator, elevationIdx);
                        }
                        else
                        {
                            var cost = Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
                        }
                    }
                }
                break;

            case BeliefEnum.InElevator:
                {
                    var elevationIdx = beliefToEnable.Idx;
                    var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
                    moveToPositionRef.Value = (true, elevationEnterPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.ElevatorCalled:
                {
                    var elevationCallIdx = beliefToEnable.Idx;
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

                        impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
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

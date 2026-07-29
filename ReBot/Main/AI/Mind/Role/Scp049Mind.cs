using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Role;

public struct Scp049Mind(BotRef botRef) : IMind
{

    public bool ProcessBeliefToEnable((BeliefEnum beliefEnum, BeliefRef beliefRef) beliefToEnable, NativeList<(float cost, (BeliefEnum beliefEnum, BeliefRef beliefRef))> enablingBeliefs)
    {
        var actionFound = false;

        switch (beliefToEnable.beliefEnum)
        {
            case BeliefEnum.TargetsTerminated:
                {
                    var enemiesWithinSight = BotStore.Bots[botRef.Idx].TerminationState.EnemiesWithinSight;
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
                    var enemiesWithinSight = BotStore.Bots[botRef.Idx].TerminationState.EnemiesWithinSight;
                    var enemyIdx = beliefToEnable.beliefRef.Idx;
                    var enemyPosition = enemiesWithinSight[enemyIdx];

                    var obstaclesByTarget = BotStore.Bots[botRef.Idx].ObstacleState.ObstaclesByTarget;
                    if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
                    {
                        var enemiesWithinDistance = BotStore.Bots[botRef.Idx].TerminationState.EnemiesWithinDistance;
                        if (enemiesWithinDistance.Contains(new EnemyRef(enemyIdx)))
                        {
                            var cardiacPositionRef = BotStore.Bots[botRef.Idx].CardiacState.CardiacPositionRef;
                            cardiacPositionRef.Value = (true, enemyPosition);

                            actionFound = true;
                        }
                        else
                        {
                            var playerPosition = BotStore.Bots[botRef.Idx].PlayerState.Position;
                            var cost = Vector3.Distance(enemyPosition, playerPosition);

                            enablingBeliefs.Add((cost, (BeliefEnum.WithinEnemyLocation, new(enemyIdx))));
                        }
                    }
                    else
                    {
                        var obstacleLocations = ObstacleState.ObstacleLocations;
                        var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                        enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx))));
                    }
                }
                break;

            case BeliefEnum.SightedEnemies:
                {
                    var roomEnterLocations = BotStore.Bots[botRef.Idx].RoomState.RoomEnterLocations;
                    if (roomEnterLocations.Length > 0)
                    {
                        for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < roomEnterLocations.Length; roomEnterLocationIdx++)
                        {
                            var roomEnterLocationPosition = roomEnterLocations[roomEnterLocationIdx];

                            var cost = 10f; // multiplier is odds of seeing enemies

                            var obstaclesByTarget = BotStore.Bots[botRef.Idx].ObstacleState.ObstaclesByTarget;
                            if (!obstaclesByTarget.TryGetValue(roomEnterLocationPosition, out var obstacleRef))
                            {
                                var playerPosition = BotStore.Bots[botRef.Idx].PlayerState.Position;
                                cost *= Vector3.Distance(roomEnterLocationPosition, playerPosition);

                                enablingBeliefs.Add((cost, (BeliefEnum.WithinRoomEnterLocation, new(roomEnterLocationIdx))));
                            }
                            else
                            {
                                var obstacleLocations = ObstacleState.ObstacleLocations;
                                cost *= Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                                enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacleRef.Idx))));
                            }
                        }
                    }
                }
                break;

            case BeliefEnum.WithinEnemyLocation:
                {
                    var enemiesWithinSight = BotStore.Bots[botRef.Idx].TerminationState.EnemiesWithinSight;
                    var targetPosition = enemiesWithinSight[beliefToEnable.beliefRef.Idx];
                    var moveToPositionRef = BotStore.Bots[botRef.Idx].MovementState.MoveToPositionRef;
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.WithinRoomEnterLocation:
                {
                    var roomEnterLocations = BotStore.Bots[botRef.Idx].RoomState.RoomEnterLocations;
                    var targetPosition = roomEnterLocations[beliefToEnable.beliefRef.Idx];
                    var moveToPositionRef = BotStore.Bots[botRef.Idx].MovementState.MoveToPositionRef;
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.ObstacleToOvercome:
                {
                    var obstacleLocations = ObstacleState.ObstacleLocations;
                    var beliefObstacleRef = beliefToEnable.beliefRef;
                    var obstaclePosition = obstacleLocations[beliefObstacleRef.Idx];

                    var elevationTravelObstacles = ObstacleState.ElevationTravelObstacles;
                    var elevationCallObstacles = ObstacleState.ElevationCallObstacles;
                    if (elevationTravelObstacles.IndexOf(new ObstacleRef(beliefToEnable.beliefRef.Idx)) is var elevationIdx && elevationIdx > -1)
                    {
                        var cost = 5f;

                        enablingBeliefs.Add((cost, (BeliefEnum.TravelingOnElevator, new(elevationIdx))));
                    }
                    else if (elevationCallObstacles.IndexOf(new ObstacleRef(beliefToEnable.beliefRef.Idx)) is var elevationCallIdx && elevationCallIdx > -1)
                    {
                        var elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;
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
                    var obstacleLocations = ObstacleState.ObstacleLocations;
                    var obstacleInteractPosition = obstacleLocations[obstacleInteractIdx];

                    var obstaclesByTarget = BotStore.Bots[botRef.Idx].ObstacleState.ObstaclesByTarget;
                    if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
                    {
                        var playerPosition = BotStore.Bots[botRef.Idx].PlayerState.Position;
                        if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
                        {
                            var interactPositionRef = BotStore.Bots[botRef.Idx].InteractionState.InteractWithPositionRef;
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
                    var obstacleLocations = ObstacleState.ObstacleLocations;
                    var targetPosition = obstacleLocations[beliefToEnable.beliefRef.Idx];
                    var moveToPositionRef = BotStore.Bots[botRef.Idx].MovementState.MoveToPositionRef;
                    moveToPositionRef.Value = (true, targetPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.TravelingOnElevator:
                {
                    var elevationIdx = beliefToEnable.beliefRef.Idx;
                    var elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
                    var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

                    var playerPosition = BotStore.Bots[botRef.Idx].PlayerState.Position;
                    if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
                    {
                        var elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations;
                        var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
                        var interactPositionRef = BotStore.Bots[botRef.Idx].InteractionState.InteractWithPositionRef;
                        interactPositionRef.Value = (true, travelInteractPosition);

                        actionFound = true;
                    }
                    else
                    {
                        var obstaclesByTarget = BotStore.Bots[botRef.Idx].ObstacleState.ObstaclesByTarget;
                        if (!obstaclesByTarget.TryGetValue(elevationEnterPosition, out var obstacle2Ref))
                        {
                            var cost = Vector3.Distance(elevationEnterPosition, playerPosition);

                            enablingBeliefs.Add((cost, (BeliefEnum.InElevator, new(elevationIdx))));
                        }
                        else
                        {
                            var obstacleLocations = ObstacleState.ObstacleLocations;
                            var cost = Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                            enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx))));
                        }
                    }
                }
                break;

            case BeliefEnum.InElevator:
                {
                    var elevationIdx = beliefToEnable.beliefRef.Idx;
                    var elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
                    var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
                    var moveToPositionRef = BotStore.Bots[botRef.Idx].MovementState.MoveToPositionRef;
                    moveToPositionRef.Value = (true, elevationEnterPosition);

                    actionFound = true;
                }
                break;

            case BeliefEnum.ElevatorCalled:
                {
                    var elevationCallIdx = beliefToEnable.beliefRef.Idx;
                    var elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;
                    var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

                    var obstaclesByTarget = BotStore.Bots[botRef.Idx].ObstacleState.ObstaclesByTarget;
                    if (!obstaclesByTarget.TryGetValue(elevationCallInteractPosition, out var obstacle2Ref))
                    {
                        var playerPosition = BotStore.Bots[botRef.Idx].PlayerState.Position;
                        if (Vector3.Distance(elevationCallInteractPosition, playerPosition) < 2f)
                        {
                            var interactPositionRef = BotStore.Bots[botRef.Idx].InteractionState.InteractWithPositionRef;
                            interactPositionRef.Value = (true, elevationCallInteractPosition);
                        }
                        else
                        {
                            var moveToPositionRef = BotStore.Bots[botRef.Idx].MovementState.MoveToPositionRef;
                            moveToPositionRef.Value = (true, elevationCallInteractPosition);
                        }

                        actionFound = true;
                    }
                    else
                    {
                        var obstacleLocations = ObstacleState.ObstacleLocations;
                        var cost = Vector3.Distance(elevationCallInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                        enablingBeliefs.Add((cost, (BeliefEnum.ObstacleToOvercome, new(obstacle2Ref.Idx))));
                    }
                }
                break;
        }

        return actionFound;
    }
}

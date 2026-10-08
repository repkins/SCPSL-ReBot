using ReBot.Main.AI.Mind.Abilities.BlinkTo;
using ReBot.Main.AI.Mind.Abilities.GoTo.Scp173;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SnapEnemyEnabling(BotRef botRef)
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> snapPositionRef = botRef.Value.NeckSnapState.SnapPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    GoTo<Enemy>.Impacting goToEnemyImpacting = new(botRef, new());
    ObservedBlinkTo<Enemy>.Impacting observedBlinkToEnemyImpacting = new(botRef, new(botRef));

    public bool Process(EnemyRef enemyRef, NativeList<ImpactingAction> impactingActions)
    {
        var enemyPosition = enemyLocations[enemyRef.Idx];

        if (enemiesWithinDistance.Contains(enemyRef))
        {
            if (!isObserved)
            {
                if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
                {
                    lookAtPositionRef.Value = (true, enemyPosition);
                    snapPositionRef.Value = (true, enemyPosition);

                    return true;
                }
                else
                {
                    var cost = 0f;
                    impactingActions.Add(cost, ActionEnum.Overcomes | ActionEnum.Obstacle, obstacleRef.Idx);
                }
            }

            observedBlinkToEnemyImpacting.Process(enemyRef.Idx, impactingActions);
        }
        else
        {
            goToEnemyImpacting.Process(enemyRef.Idx, impactingActions);
        }

        return false;
    }
}

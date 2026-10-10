using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct CaptureEnemy(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> capturePositionRef = botRef.Value.CaptureState.CapturePositionRef;

    public bool Impacting(EnemyRef enemyRef, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 1f;  // attack cost, constant

        impactingActions.Add(cost, ActionEnum.Attacks | ActionEnum.Enemy, enemyRef.Idx);

        return false;
    }

    public bool Enabling(EnemyRef enemyRef, NativeList<BeliefRef> beliefsToEnable)
    {
        var enemyPosition = enemyLocations[enemyRef.Idx];

        if (enemiesWithinDistance.Contains(enemyRef))
        {
            if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
            {
                lookAtPositionRef.Value = (true, enemyPosition);
                capturePositionRef.Value = (true, enemyPosition);

                return true;
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacleRef.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinEnemy, enemyRef.Idx));
        }

        return false;
    }
}

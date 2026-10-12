using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct CardiacEnemy(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;
    NativeList<EnemyRef> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> cardiacPositionRef = botRef.Value.CardiacState.CardiacPositionRef;

    public void Impacting(EnemyRef enemyRef, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 1f;  // attack cost, constant

        impactingActions.Add(cost, ActionEnum.Attacks | ActionEnum.Enemy, enemyRef.Idx);
    }

    public void Enabling(EnemyRef enemyRef, NativeList<BeliefRef> beliefsToEnable)
    {
        var enemyPosition = enemyLocations[enemyRef.Idx];

        if (enemiesWithinDistance.Contains(enemyRef))
        {
            if (enemiesWithinSight.Contains(enemyRef))
            {
                if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
                {
                    lookAtPositionRef.Value = (true, enemyPosition);
                    cardiacPositionRef.Value = (true, enemyPosition);
                }
                else
                {
                    beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacleRef.Idx));
                }
            }
            else
            {
                lookAtPositionRef.Value = (true, enemyPosition);
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinEnemy, enemyRef.Idx));
        }
    }
}

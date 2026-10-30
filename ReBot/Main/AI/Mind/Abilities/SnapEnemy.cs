using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SnapEnemy(BotRef botRef)
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;

    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;
    NativeList<Vector3> enemyVelocities = botRef.Value.TerminationState.EnemyVelocities;
    NativeList<EnemyRef> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> snapPositionRef = botRef.Value.NeckSnapState.SnapPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public void Impacting(EnemyRef enemyRef, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 0f;  // snap attack cost, instant

        impactingActions.Add(cost, ActionEnum.Attacks | ActionEnum.Enemy, enemyRef.Idx);
    }

    public void Enabling(EnemyRef enemyRef, NativeList<BeliefRef> beliefsToEnable)
    {
        var enemyPosition = enemyLocations[enemyRef.Idx];

        if (enemiesWithinDistance.Contains(enemyRef) && !isObserved)
        {
            if (enemiesWithinSight.Contains(enemyRef))
            {
                if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
                {
                    lookAtPositionRef.Value = (true, enemyPosition);
                    snapPositionRef.Value = (true, enemyPosition);
                }
                else
                {
                    beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacleRef.Idx));
                }
            }
            else
            {
                var enemyVelocity = enemyVelocities[enemyRef.Idx];
                lookAtPositionRef.Value = (true, enemyPosition + enemyVelocity);
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinEnemy, enemyRef.Idx));
        }
    }
}

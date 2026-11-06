using ReBot.Main.AI.State.Spacial;
using ReBot.Main.AI.State.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities;

internal struct MeleeEnemy(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;
    NativeList<Vector3> enemyVelocities = botRef.Value.TerminationState.EnemyVelocities;
    NativeList<EnemyRef> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> meleeAttackPositionRef = botRef.Value.MeleeState.MeleeAttackPositionRef;

    public void Impacting(EnemyRef enemyRef, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 1f;  // melee attack cost, constant

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
                    meleeAttackPositionRef.Value = (true, enemyPosition);
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

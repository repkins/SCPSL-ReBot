using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Scp096MeleeableEnemy(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;
    bool isEnraged = botRef.Value.RageState.IsEnraged;
    bool hasTargetsObservedBy = botRef.Value.RageState.HasTargets;

    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> meleeAttackPositionRef = botRef.Value.Scp096MeleeState.MeleeAttackPositionRef;

    public bool Terminates(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (isEnraged)
        {
            if (hasTargetsObservedBy)
            {
                for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
                {
                    var cost = 1f;  // melee attack cost, constant

                    impactingBeliefs.Add(cost, BeliefEnum.AttackedEnemy, enemyIdx);
                }
            }
            else
            {
                var cost = 5f;  // calming cost

                impactingBeliefs.Add(cost, BeliefEnum.Calmed, default);
            }
        }
        else if (hasTargetsObservedBy)
        {
            var cost = 5f;  // entering rage cost

            impactingBeliefs.Add(cost, BeliefEnum.Enraged, default);
        }
        else
        {
            var cost = 10f; // max sight distance, enraged

            //impactingBeliefs.Add(cost, BeliefEnum.SightedEnemies, default);
        }

        return false;
    }

    public bool Attacks(EnemyRef enemyRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var enemyPosition = enemiesWithinSight[enemyRef.Idx];

        if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
        {
            if (enemiesWithinDistance.Contains(enemyRef))
            {
                lookAtPositionRef.Value = (true, enemyPosition);
                meleeAttackPositionRef.Value = (true, enemyPosition);

                return true;
            }
            else
            {
                var cost = Vector3.Distance(enemyPosition, playerPosition);

                impactingBeliefs.Add(cost, BeliefEnum.MovedToEnemyLocation, enemyRef.Idx);
            }
        }
        else
        {
            var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
        }

        return false;
    }

    public bool MovesTo(EnemyRef enemyRef)
    {
        var targetPosition = enemiesWithinSight[enemyRef.Idx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}

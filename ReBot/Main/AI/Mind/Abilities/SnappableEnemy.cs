using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SnappableEnemy(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> snapPositionRef = botRef.Value.NeckSnapState.SnapPositionRef;

    public bool Terminates(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (enemiesWithinSight.Length > 0)
        {
            for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
            {
                var cost = 0f;  // snap attack cost, instant

                impactingBeliefs.Add(cost, BeliefEnum.AttackedEnemy, enemyIdx);
            }
        }
        else
        {
            var cost = 30; // max sight distance

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
                snapPositionRef.Value = (true, enemyPosition);

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

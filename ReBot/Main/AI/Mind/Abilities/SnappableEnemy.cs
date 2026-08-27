using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SnappableEnemy(BotRef botRef)
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> snapPositionRef = botRef.Value.NeckSnapState.SnapPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

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

            impactingBeliefs.Add(cost, BeliefEnum.SightedEnemies, default);
        }

        return false;
    }

    public bool Attacks(EnemyRef enemyRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var enemyPosition = enemiesWithinSight[enemyRef.Idx];

        if (enemiesWithinDistance.Contains(enemyRef))
        {
            if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
            {
                if (!isObserved)
                {
                    lookAtPositionRef.Value = (true, enemyPosition);
                    snapPositionRef.Value = (true, enemyPosition);

                    return true;
                }

                if (isObserved)
                {
                    var cost = remainingSecondsToBlink;

                    impactingBeliefs.Add(cost, BeliefEnum.BlinkedToEnemy, enemyRef.Idx);
                }
            }
            else
            {
                var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
            }
        }
        else
        {
            var cost = 0f;

            impactingBeliefs.Add(cost, BeliefEnum.WithinEnemyLocation, enemyRef.Idx);
        }

        return false;
    }
}

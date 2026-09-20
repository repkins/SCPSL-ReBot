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

    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> snapPositionRef = botRef.Value.NeckSnapState.SnapPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    GoTo<Enemy>.Impacting goToEnemyImpacting = new(botRef, new());

    public bool Process(EnemyRef enemyRef, NativeList<ImpactingBelief> impactingBeliefs)
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

                    impactingBeliefs.Add(cost, BeliefEnum.BlinkedTo | BeliefEnum.Enemy, enemyRef.Idx);
                }
            }
            else
            {
                var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

                impactingBeliefs.Add(cost, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacleRef.Idx);
            }
        }
        else
        {
            goToEnemyImpacting.Process(enemyRef.Idx, impactingBeliefs);
        }

        return false;
    }
}

using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct CardiacEnemyEnabling(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;
    NativeList<Vector3> enemyVelocities = botRef.Value.TerminationState.EnemyVelocities;
    NativeList<EnemyRef> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> cardiacPositionRef = botRef.Value.CardiacState.CardiacPositionRef;

    MoveTo<Enemy>.Impacting moveToEnemyImpacting = new(botRef, new(botRef));

    public bool Process(EnemyRef enemyRef, NativeList<ImpactingBelief> impactingBeliefs)
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

                    return true;
                }
                else
                {
                    var cost = 0f;
                    impactingBeliefs.Add(cost, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacleRef.Idx);
                }
            }
            else
            {
                var estimatedEnemyPosition = enemyPosition + enemyVelocities[enemyRef.Idx];
                lookAtPositionRef.Value = (true, estimatedEnemyPosition);

                return true;
            }
        }
        else
        {
            moveToEnemyImpacting.Process(enemyRef.Idx, impactingBeliefs);
        }

        return false;
    }
}

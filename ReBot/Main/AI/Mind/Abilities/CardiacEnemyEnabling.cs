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
    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;
    NativeList<EnemyRef> enemiesWithinDistance = botRef.Value.TerminationState.EnemiesWithinDistance;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> cardiacPositionRef = botRef.Value.CardiacState.CardiacPositionRef;

    MoveTo<Enemy>.Impacting moveToEnemyImpacting = new(botRef, new(botRef));

    public bool Process(EnemyRef enemyRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var enemyPosition = enemiesWithinSight[enemyRef.Idx];

        if (!obstaclesByTarget.TryGetValue(enemyPosition, out var obstacleRef))
        {
            if (enemiesWithinDistance.Contains(enemyRef))
            {
                lookAtPositionRef.Value = (true, enemyPosition);
                cardiacPositionRef.Value = (true, enemyPosition);

                return true;
            }
            else
            {
                moveToEnemyImpacting.Process(enemyRef.Idx, impactingBeliefs);
            }
        }
        else
        {
            var cost = Vector3.Distance(enemyPosition, obstacleLocations[obstacleRef.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacleRef.Idx);
        }

        return false;
    }
}

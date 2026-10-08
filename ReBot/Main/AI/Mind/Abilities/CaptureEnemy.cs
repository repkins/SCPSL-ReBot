using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
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

    MovedTo<Enemy> movedToEnemy = new(botRef, new(botRef));

    public bool Process(EnemyRef enemyRef, NativeList<ImpactingAction> impactingActions)
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
                var cost = 0f;
                impactingActions.Add(cost, ActionEnum.Overcomes | ActionEnum.Obstacle, obstacleRef.Idx);
            }
        }
        else
        {
            movedToEnemy.Process(enemyRef.Idx, impactingActions);
        }

        return false;
    }
}

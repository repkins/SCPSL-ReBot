using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct TerminateEnemyEnabling(BotRef botRef)
{
    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;

    public bool Process(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (enemiesWithinSight.Length > 0)
        {
            for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
            {
                var cost = 1f;  // melee attack cost, constant

                impactingBeliefs.Add(cost, BeliefEnum.AttackedEnemy, enemyIdx);
            }
        }
        else
        {
            var cost = 30f; // max sight distance

            //impactingBeliefs.Add(cost, BeliefEnum.SightedEnemies, default);
        }

        return false;
    }
}

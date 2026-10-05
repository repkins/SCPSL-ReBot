using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate;

internal struct TerminateEnemyEnabling(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;

    public bool Process(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (enemyLocations.Length > 0)
        {
            for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
            {
                var cost = 1f;  // attack cost, constant

                impactingBeliefs.Add(cost, BeliefEnum.Attacked | BeliefEnum.Enemy, enemyIdx);
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

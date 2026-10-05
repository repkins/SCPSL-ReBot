using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate.Scp173;

internal struct TerminateEnemyEnabling(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;

    public bool Process(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (enemyLocations.Length > 0)
        {
            for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
            {
                var cost = 0f;  // snap attack cost, instant

                impactingBeliefs.Add(cost, BeliefEnum.Attacked | BeliefEnum.Enemy, enemyIdx);
            }
        }
        else
        {
            var cost = 30; // max sight distance

            impactingBeliefs.Add(cost, BeliefEnum.Sighted | BeliefEnum.Enemy, default);
        }

        return false;
    }
}

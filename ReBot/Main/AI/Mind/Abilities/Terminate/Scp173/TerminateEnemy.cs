using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate.Scp173;

internal struct TerminateEnemy(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;

    public bool Process(NativeList<ImpactingAction> impactingActions)
    {
        if (enemyLocations.Length > 0)
        {
            for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
            {
                var cost = 0f;  // snap attack cost, instant

                impactingActions.Add(cost, ActionEnum.Attacks | ActionEnum.Enemy, enemyIdx);
            }
        }
        else
        {
            var cost = 30; // max sight distance

            impactingActions.Add(cost, ActionEnum.Finds | ActionEnum.Enemy, default);
        }

        return false;
    }
}

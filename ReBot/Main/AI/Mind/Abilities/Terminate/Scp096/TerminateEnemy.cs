using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate.Scp096;

internal struct TerminateEnemy(BotRef botRef)
{
    bool isEnraged = botRef.Value.RageState.IsEnraged;
    bool hasTargetsObservedBy = botRef.Value.RageState.HasTargets;

    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;

    public bool Process(NativeList<ImpactingAction> impactingActions)
    {
        if (isEnraged)
        {
            if (hasTargetsObservedBy)
            {
                for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
                {
                    var cost = 1f;  // melee attack cost, constant

                    impactingActions.Add(cost, ActionEnum.Attacks | ActionEnum.Enemy, enemyIdx);
                }
            }
            else
            {
                var cost = 5f;  // calming cost

                impactingActions.Add(cost, ActionEnum.Calms, default);
            }
        }
        else if (hasTargetsObservedBy)
        {
            var cost = 5f;  // entering rage cost

            impactingActions.Add(cost, ActionEnum.Enrages, default);
        }
        else
        {
            var cost = 10f; // max sight distance, enraged

            //impactingActions.Add(cost, ActionEnum.SightedEnemies, default);
        }

        return false;
    }
}

using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate.Scp096;

internal struct TerminateEnemyEnabling(BotRef botRef)
{
    bool isEnraged = botRef.Value.RageState.IsEnraged;
    bool hasTargetsObservedBy = botRef.Value.RageState.HasTargets;

    NativeList<Vector3> enemiesWithinSight = botRef.Value.TerminationState.EnemiesWithinSight;

    public bool Process(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (isEnraged)
        {
            if (hasTargetsObservedBy)
            {
                for (var enemyIdx = 0; enemyIdx < enemiesWithinSight.Length; enemyIdx++)
                {
                    var cost = 1f;  // melee attack cost, constant

                    impactingBeliefs.Add(cost, BeliefEnum.Attacked | BeliefEnum.Enemy, enemyIdx);
                }
            }
            else
            {
                var cost = 5f;  // calming cost

                impactingBeliefs.Add(cost, BeliefEnum.Calmed, default);
            }
        }
        else if (hasTargetsObservedBy)
        {
            var cost = 5f;  // entering rage cost

            impactingBeliefs.Add(cost, BeliefEnum.Enraged, default);
        }
        else
        {
            var cost = 10f; // max sight distance, enraged

            //impactingBeliefs.Add(cost, BeliefEnum.SightedEnemies, default);
        }

        return false;
    }
}

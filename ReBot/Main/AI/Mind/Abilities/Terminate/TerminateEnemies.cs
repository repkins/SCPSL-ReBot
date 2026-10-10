using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate;

internal struct TerminateEnemies(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;

    public bool Enabling(NativeList<BeliefRef> beliefsToEnable)
    {
        if (enemyLocations.Length > 0)
        {
            for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
            {
                beliefsToEnable.Add(new(BeliefEnum.TerminatedEnemy, enemyIdx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.SightedEnemies, default));
        }

        return false;
    }
}

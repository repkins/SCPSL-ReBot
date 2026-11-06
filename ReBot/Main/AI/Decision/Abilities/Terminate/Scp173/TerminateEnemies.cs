using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Terminate.Scp173;

internal struct TerminateEnemies(BotRef botRef)
{
    NativeList<Vector3> enemyLocations = botRef.Value.TerminationStore.EnemyLocations;

    public void Enabling(NativeList<BeliefRef> beliefsToEnable)
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
    }
}

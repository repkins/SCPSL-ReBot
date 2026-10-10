using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Terminate.Scp096;

internal struct TerminateEnemies(BotRef botRef)
{
    bool isEnraged = botRef.Value.RageState.IsEnraged;
    bool hasTargetsObservedBy = botRef.Value.RageState.HasTargets;

    NativeList<Vector3> enemyLocations = botRef.Value.TerminationState.EnemyLocations;

    public bool Enabling(NativeList<BeliefRef> beliefsToEnable)
    {
        if (isEnraged)
        {
            if (hasTargetsObservedBy)
            {
                for (var enemyIdx = 0; enemyIdx < enemyLocations.Length; enemyIdx++)
                {
                    beliefsToEnable.Add(new(BeliefEnum.TerminatedEnemy, enemyIdx));
                }
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.Calmed, default));
            }
        }
        else if (hasTargetsObservedBy)
        {
            beliefsToEnable.Add(new(BeliefEnum.Enraged, default));
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.SightedEnemies, default));
        }

        return false;
    }
}

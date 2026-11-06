using ReBot.Main.AI.State.Termination;
using Unity.Collections;

namespace ReBot.Main.AI.Decision.Abilities.Terminate.Scp096;

internal struct TerminateEnemies(BotRef botRef)
{
    bool isEnraged = botRef.Value.RageState.IsEnraged;
    bool hasTargetsObservedBy = botRef.Value.RageState.HasTargets;

    NativeList<EnemyRef> targets = botRef.Value.RageState.Targets;

    public void Enabling(NativeList<BeliefRef> beliefsToEnable)
    {
        if (isEnraged)
        {
            if (hasTargetsObservedBy)
            {
                for (var targetIdx = 0; targetIdx < targets.Length; targetIdx++)
                {
                    var enemyRef = targets[targetIdx];
                    beliefsToEnable.Add(new(BeliefEnum.TerminatedEnemy, enemyRef.Idx));
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
    }
}

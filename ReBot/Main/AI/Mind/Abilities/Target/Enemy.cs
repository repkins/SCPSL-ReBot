using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Target;

internal struct Enemy(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.TerminationState.EnemiesWithinSight;

    public BeliefEnum Enum { get; } = BeliefEnum.Enemy;
}

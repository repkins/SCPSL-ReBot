using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink.Location;

internal struct EnemyLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.TerminationState.EnemiesWithinSight;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToEnemy;
}


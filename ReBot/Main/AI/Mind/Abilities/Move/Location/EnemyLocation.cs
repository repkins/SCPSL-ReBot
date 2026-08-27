using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move.Location;

internal struct EnemyLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.TerminationState.EnemiesWithinSight;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToEnemyLocation;
}


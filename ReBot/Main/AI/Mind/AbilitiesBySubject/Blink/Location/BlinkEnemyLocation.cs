using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.AbilitiesBySubject.Blink.Location;

internal struct BlinkEnemyLocation(BotRef botRef) : IBlinkTarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.TerminationState.EnemiesWithinSight;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToEnemy;
}


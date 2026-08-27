using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink.Location;

internal struct BlinkElevationCallObstacleLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationCallInteractLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToElevatorCallLocation;
}

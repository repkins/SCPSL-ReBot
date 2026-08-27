using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink.Location;

internal struct BlinkElevationTravelObstacleEnterLocation(BotRef botRef) : IBlinkTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToElevator;
}

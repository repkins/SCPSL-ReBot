using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink.Location;

internal struct ElevationTravelObstacleEnterLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToElevator;
}

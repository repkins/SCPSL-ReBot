using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.AbilitiesBySubject.Blink.Location;

internal struct BlinkElevationTravelObstacleEnterLocation() : IBlinkTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToElevator;
}

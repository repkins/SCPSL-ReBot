using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.AbilitiesBySubject.Blink.Location;

internal struct BlinkElevationCallObstacleLocation() : IBlinkTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationCallInteractLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToElevatorCallLocation;
}

using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.AbilitiesBySubject.Blink.Location;

internal struct BlinkInteractableObstacleLocation() : IBlinkTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ObstacleLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToObstacleInteractionLocation;
}

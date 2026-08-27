using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink.Location;

internal struct InteractableObstacleLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ObstacleLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToObstacleInteractionLocation;
}

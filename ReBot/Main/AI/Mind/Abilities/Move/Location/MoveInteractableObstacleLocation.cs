using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move.Location;

internal struct MoveInteractableObstacleLocation(BotRef botRef) : IMoveTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ObstacleLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToObstacleInteractionLocation;
}

using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move.Location;

internal struct MoveElevationCallObstacleLocation(BotRef botRef) : IMoveTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationCallInteractLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToElevatorCallLocation;
}

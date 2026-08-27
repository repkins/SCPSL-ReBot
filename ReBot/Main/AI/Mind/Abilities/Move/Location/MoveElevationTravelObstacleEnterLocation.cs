using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move.Location;

internal struct MoveElevationTravelObstacleEnterLocation(BotRef botRef) : IMoveTarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToElevator;
}

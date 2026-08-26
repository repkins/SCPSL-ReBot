using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableToElevationTravelObstacleEnterLocation(BotRef botRef) : IMovableToTarget
{
    MovableTo<ElevationTravelObstacleEnterLocation> movableToElevationTravelObstacleEnterLocation = new(botRef, new(botRef));

    public bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return movableToElevationTravelObstacleEnterLocation.GoesTo(targetLocationIdx, impactingBeliefs);
    }

    public bool MovesTo(int targetLocationIdx)
    {
        return movableToElevationTravelObstacleEnterLocation.MovesTo(targetLocationIdx);
    }
}

internal struct ElevationTravelObstacleEnterLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToElevator;
}

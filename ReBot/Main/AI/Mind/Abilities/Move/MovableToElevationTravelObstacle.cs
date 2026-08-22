using ReBot.Main.AI.Spacial;
using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableToElevationTravelObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;
    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool GoesToElevator(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
        var cost = Vector3.Distance(elevationEnterPosition, playerPosition);

        impactingBeliefs.Add(cost, BeliefEnum.MovedToElevator, elevationIdx);

        return false;
    }

    public bool MovesToElevator(int elevationIdx)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
        moveToPositionRef.Value = (true, elevationEnterPosition);

        return true;
    }
}

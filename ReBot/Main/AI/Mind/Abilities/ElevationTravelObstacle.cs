using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct ElevationTravelObstacle()
{
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    public void Overcomes(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = 5f;

        impactingBeliefs.Add(cost, BeliefEnum.TravelingOnElevator, elevationIdx);
    }
}

using ReBot.Main.AI.Spacial;
using System.Runtime.Remoting;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct OvercomesElevationCallObstacle()
{
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<Vector3> elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;

    public void Process(ObstacleRef obstacleRef, int elevationCallIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];
        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

        var cost = Vector3.Distance(obstaclePosition, elevationCallInteractPosition) + 5f;

        impactingBeliefs.Add(cost, BeliefEnum.ElevatorCalled, elevationCallIdx);
    }
}

using ReBot.Main.AI.Spacial;
using System.Runtime.Remoting;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct OvercomesInteractableObstacle()
{
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    public void Process(ObstacleRef obstacleRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];
        var obstacleInteractPosition = obstacleLocations[obstacleRef.Idx];

        var cost = Vector3.Distance(obstaclePosition, obstacleInteractPosition) + 1f;

        impactingBeliefs.Add(cost, BeliefEnum.InteractingWithObstacle, obstacleRef.Idx);
    }
}

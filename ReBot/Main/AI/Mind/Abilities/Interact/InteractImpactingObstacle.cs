using Interactables.Interobjects.DoorUtils;
using ReBot.Main.AI.Spacial;
using System.Runtime.Remoting;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Interact;

internal struct InteractImpactingObstacle()
{
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    public bool Process(ObstacleRef obstacleRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];
        var obstacleInteractPosition = obstacleLocations[obstacleRef.Idx];

        var cost = Vector3.Distance(obstaclePosition, obstacleInteractPosition) + 1f;

        impactingBeliefs.Add(cost, BeliefEnum.InteractingWith | BeliefEnum.Obstacle, obstacleRef.Idx);

        return false;
    }
}

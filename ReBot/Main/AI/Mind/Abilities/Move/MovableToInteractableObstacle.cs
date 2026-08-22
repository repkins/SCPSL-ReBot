using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableToInteractableObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool GoesTo(int obstacleInteractIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstacleInteractPosition = obstacleLocations[obstacleInteractIdx];
        var cost = Vector3.Distance(obstacleInteractPosition, playerPosition);

        impactingBeliefs.Add(cost, BeliefEnum.MovedToObstacleInteractionLocation, obstacleInteractIdx);

        return false;
    }

    public bool MovesTo(int obstacleInteractIdx)
    {
        var targetPosition = obstacleLocations[obstacleInteractIdx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}

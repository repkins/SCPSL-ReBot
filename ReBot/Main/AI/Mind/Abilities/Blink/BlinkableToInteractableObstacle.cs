using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink;

internal struct BlinkableToInteractableObstacle(BotRef botRef)
{
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

    public bool GoesTo(int obstacleInteractIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = remainingSecondsToBlink;

        impactingBeliefs.Add(cost, BeliefEnum.BlinkedToObstacleInteractionLocation, obstacleInteractIdx);

        return false;
    }

    public bool BlinksTo(int obstacleInteractIdx)
    {
        var targetPosition = obstacleLocations[obstacleInteractIdx];
        blinkToPositionRef.Value = (true, targetPosition);

        return true;
    }
}

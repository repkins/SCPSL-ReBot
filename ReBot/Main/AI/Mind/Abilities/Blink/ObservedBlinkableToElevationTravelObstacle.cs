using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink;

internal struct ObservedBlinkableToElevationTravelObstacle(BotRef botRef)
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

    public bool GoesTo(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (isObserved)
        {
            var cost = remainingSecondsToBlink;

            impactingBeliefs.Add(cost, BeliefEnum.BlinkedToElevator, elevationIdx);
        }

        return false;
    }

    public bool BlinksTo(int elevationIdx)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
        blinkToPositionRef.Value = (true, elevationEnterPosition);

        return true;
    }
}

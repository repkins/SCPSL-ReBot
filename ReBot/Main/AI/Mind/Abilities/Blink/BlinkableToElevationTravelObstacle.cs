using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink;

internal struct BlinkableToElevationTravelObstacle(BotRef botRef)
{
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

    public bool GoesToElevator(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = remainingSecondsToBlink;

        impactingBeliefs.Add(cost, BeliefEnum.BlinkedToElevator, elevationIdx);

        return false;
    }

    public bool BlinksToElevator(int elevationIdx)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
        blinkToPositionRef.Value = (true, elevationEnterPosition);

        return true;
    }
}

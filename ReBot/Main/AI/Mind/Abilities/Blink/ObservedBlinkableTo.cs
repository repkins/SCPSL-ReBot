using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink;

internal struct ObservedBlinkableTo<TTarget>(BotRef botRef, TTarget target)
    where TTarget : IBlinkTarget
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

    public bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (isObserved)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
            {
                var cost = remainingSecondsToBlink;

                impactingBeliefs.Add(cost, target.BlinkedToEnum, targetLocationIdx);
            }
            else
            {
                var cost = remainingSecondsToBlink;

                impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
            }
        }

        return false;
    }

    public bool BlinksTo(int targetLocationIdx)
    {
        var targetPosition = target.Locations[targetLocationIdx];
        blinkToPositionRef.Value = (true, targetPosition);

        return true;
    }
}

internal interface IBlinkTarget
{
    NativeList<Vector3> Locations { get; }
    BeliefEnum BlinkedToEnum { get; }
}

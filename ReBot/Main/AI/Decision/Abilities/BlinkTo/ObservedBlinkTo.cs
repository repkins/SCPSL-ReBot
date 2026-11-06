using ReBot.Main.AI.Decision.Abilities.Target;
using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.BlinkTo;

internal struct ObservedBlinkTo<TTarget>(BotRef botRef, TTarget target)
    where TTarget : ITarget
{
    bool isObserved = botRef.Value.BlinkStore.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkStore.RemainingSecondsToBlink;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleStore.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkStore.BlinkToPositionRef;

    public void Impacting(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        if (isObserved)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            var cost = !obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef)
                ? remainingSecondsToBlink
                : remainingSecondsToBlink;

            impactingActions.Add(cost, ActionEnum.BlinksTo | target.Enum, targetLocationIdx);
        }
    }

    public void Enabling(int targetLocationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var targetPosition = target.Locations[targetLocationIdx];

        if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
        {
            blinkToPositionRef.Value = (true, targetPosition);
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacleRef.Idx));
        }
    }
}

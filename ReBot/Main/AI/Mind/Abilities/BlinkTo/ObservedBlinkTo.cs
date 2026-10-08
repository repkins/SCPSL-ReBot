using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.BlinkTo;

internal partial struct ObservedBlinkedTo<TTarget>(BotRef botRef, TTarget target)
    where TTarget : ITarget
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier = 1f)
    {
        if (isObserved)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            var cost = ! obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef)
                ? remainingSecondsToBlink
                : remainingSecondsToBlink;

            impactingActions.Add(cost, multiplier, ActionEnum.BlinksTo | target.Enum, targetLocationIdx);
        }

        return false;
    }
}

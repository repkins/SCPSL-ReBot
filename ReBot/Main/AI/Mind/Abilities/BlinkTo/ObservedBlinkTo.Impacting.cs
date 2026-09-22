using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.BlinkTo;

internal partial struct ObservedBlinkTo<TTarget>
    where TTarget : ITarget
{
    internal struct Impacting(BotRef botRef, TTarget target)
    {
        bool isObserved = botRef.Value.BlinkState.IsObserved;
        float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

        NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

        public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs, float multiplier = 1f)
        {
            if (isObserved)
            {
                var targetPosition = target.Locations[targetLocationIdx];

                var cost = ! obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef)
                    ? remainingSecondsToBlink
                    : remainingSecondsToBlink;

                impactingBeliefs.Add(cost, multiplier, BeliefEnum.BlinkedTo | target.Enum, targetLocationIdx);
            }

            return false;
        }
    }
}

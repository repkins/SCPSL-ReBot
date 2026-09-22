using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.BlinkTo;

internal partial struct BlinkTo<TTarget>
    where TTarget : ITarget
{
    internal struct Enabling(BotRef botRef, TTarget target)
    {
        NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

        NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

        public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
            {
                blinkToPositionRef.Value = (true, targetPosition);
                return true;
            }
            else
            {
                var cost = 0f;
                impactingBeliefs.Add(cost, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacleRef.Idx);
            }

            return false;
        }
    }
}

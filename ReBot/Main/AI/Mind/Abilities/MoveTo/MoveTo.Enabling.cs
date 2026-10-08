using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal partial struct MoveTo<TTarget>
    where TTarget : ITarget
{
    internal struct Enabling(BotRef botRef, TTarget target)
    {
        NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

        NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

        public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
            {
                moveToPositionRef.Value = (true, targetPosition);
                return true;
            }
            else
            {
                var cost = 0f;
                impactingActions.Add(cost, ActionEnum.Overcomes | ActionEnum.Obstacle, obstacleRef.Idx);
            }

            return false;
        }
    }
}

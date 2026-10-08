using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.BlinkTo;

internal struct BlinkTo<TTarget>(BotRef botRef, TTarget target)
    where TTarget : ITarget
{
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
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
            impactingActions.Add(cost, ActionEnum.Overcomes | ActionEnum.Obstacle, obstacleRef.Idx);
        }

        return false;
    }
}

using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal struct MoveTo<TTarget>(BotRef botRef, TTarget target) : IMoveTo
    where TTarget : ITarget
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public void Impacting(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        var targetPosition = target.Locations[targetLocationIdx];

        var cost = !obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef)
            ? Vector3.Distance(targetPosition, playerPosition)
            : Vector3.Distance(targetPosition, obstacleLocations[obstacleRef.Idx]);

        impactingActions.Add(cost, ActionEnum.MovesTo | target.Enum, targetLocationIdx);
    }

    public void Enabling(int targetLocationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var targetPosition = target.Locations[targetLocationIdx];

        if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
        {
            moveToPositionRef.Value = (true, targetPosition);
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacleRef.Idx));
        }
    }
}

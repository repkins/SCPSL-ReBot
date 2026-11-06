using ReBot.Main.AI.Decision;
using ReBot.Main.AI.Decision.Abilities.Target;
using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.MoveTo;

internal struct MoveTo<TTarget>(BotRef botRef, TTarget target) : IMoveTo
    where TTarget : ITarget
{
    Vector3 playerPosition = botRef.Value.PlayerStore.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleStore.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementStore.MoveToPositionRef;

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

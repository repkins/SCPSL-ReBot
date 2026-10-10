using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Passthrough;

internal struct PassthroughObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool Impacting(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];

        var cost = 1f;
        cost += !obstaclesByTarget.TryGetValue(obstaclePosition, out var obstacle2Ref)
            ? Vector3.Distance(obstaclePosition, playerPosition)
            : Vector3.Distance(obstaclePosition, obstacleLocations[obstacle2Ref.Idx]);

        impactingActions.Add(cost, ActionEnum.PassesThrough | ActionEnum.Obstacle, obstacleRef.Idx);

        return false;
    }

    public bool Enabling(int obstacleIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var obstaclePosition = obstacleLocations[obstacleIdx];

        if (!obstaclesByTarget.TryGetValue(obstaclePosition, out var obstacle2Ref))
        {
            moveToPositionRef.Value = (true, obstaclePosition);
            lookAtPositionRef.Value = (true, obstaclePosition);

            return true;
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacle2Ref.Idx));
        }

        return false;
    }
}

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

    public void Impacting(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 1f;

        impactingActions.Add(cost, ActionEnum.PassesThrough | ActionEnum.Obstacle, obstacleRef.Idx);
    }

    public void Enabling(int obstacleIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var obstaclePosition = obstacleLocations[obstacleIdx];

        if (Vector3.Distance(obstaclePosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(obstaclePosition, out var obstacle2Ref))
            {
                moveToPositionRef.Value = (true, obstaclePosition);
                lookAtPositionRef.Value = (true, obstaclePosition);
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacle2Ref.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinObstacle, obstacleIdx));
        }
    }
}

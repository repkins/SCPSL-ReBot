using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Passthrough;

internal struct PassthroughObstacleImpacting(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    public bool Process(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];

        var cost = 1f;

        if (!obstaclesByTarget.TryGetValue(obstaclePosition, out var obstacle2Ref))
        {
            cost += Vector3.Distance(obstaclePosition, playerPosition);

            impactingActions.Add(cost, ActionEnum.PassesThrough | ActionEnum.Obstacle, obstacleRef.Idx);
        }
        else
        {
            cost += Vector3.Distance(obstaclePosition, obstacleLocations[obstacle2Ref.Idx]);

            impactingActions.Add(cost, ActionEnum.Overcomes | ActionEnum.Obstacle, obstacle2Ref.Idx);
        }

        return false;
    }
}

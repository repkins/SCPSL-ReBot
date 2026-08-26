using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Obstacle;

internal struct PassthroughObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool Overcomes(ObstacleRef obstacleRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = 1f;

        impactingBeliefs.Add(cost, BeliefEnum.PassingThroughObstacle, obstacleRef.Idx);

        return false;
    }

    public bool PassesThrough(int obstacleIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstaclePosition = obstacleLocations[obstacleIdx];

        if (!obstaclesByTarget.TryGetValue(obstaclePosition, out var obstacle2Ref))
        {
            var cost = Vector3.Distance(obstaclePosition, playerPosition);

            impactingBeliefs.Add(cost, BeliefEnum.MovedToObstacleLocation, obstacleIdx);
        }
        else
        {
            var cost = Vector3.Distance(obstaclePosition, obstacleLocations[obstacle2Ref.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
        }

        return false;
    }

    public bool MovesLooksTo(int obstacleIdx)
    {
        var obstaclePosition = obstacleLocations[obstacleIdx];
        moveToPositionRef.Value = (true, obstaclePosition);
        lookAtPositionRef.Value = (true, obstaclePosition);

        return true;
    }
}

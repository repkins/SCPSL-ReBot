using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct InteractsWithObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;

    public bool Process(int obstacleInteractIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstacleInteractPosition = obstacleLocations[obstacleInteractIdx];

        if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
        {
            if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
            {
                interactPositionRef.Value = (true, obstacleInteractPosition);

                return true;
            }
            else
            {
                var cost = Vector3.Distance(obstacleInteractPosition, playerPosition);

                impactingBeliefs.Add(cost, BeliefEnum.WithinObstacleInteractionLocation, obstacleInteractIdx);
            }
        }
        else
        {
            var cost = Vector3.Distance(obstacleInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
        }

        return false;
    }
}

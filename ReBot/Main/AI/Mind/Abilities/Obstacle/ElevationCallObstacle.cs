using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Obstacle;

public struct ElevationCallObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<ObstacleRef> elevationCallObstacles = ObstacleState.ElevationCallObstacles;
    NativeList<Vector3> elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;
    NativeList<bool> elevationCallInteractAbles = ObstacleState.ElevationCallInteractAbles;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool TryGetElevationCallIdx(ObstacleRef obstacleRef, out int elevationCallIdx)
    {
        if (elevationCallObstacles.IndexOf(obstacleRef) is var resultElevationCallIdx && resultElevationCallIdx > -1)
        {
            elevationCallIdx = resultElevationCallIdx;
            return true;
        }

        elevationCallIdx = default;
        return false;
    }

    public bool Overcomes(ObstacleRef obstacleRef, int elevationCallIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = 5f;

        impactingBeliefs.Add(cost, BeliefEnum.ElevatorCalled, elevationCallIdx);

        return false;
    }

    public bool Calls(int elevationCallIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];
        var elevationCallInteractAble = elevationCallInteractAbles[elevationCallIdx];

        if (Vector3.Distance(elevationCallInteractPosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(elevationCallInteractPosition, out var obstacle2Ref))
            {
                if (elevationCallInteractAble)
                {
                    lookAtPositionRef.Value = (true, elevationCallInteractPosition);
                    interactPositionRef.Value = (true, elevationCallInteractPosition);
                }
                else
                {
                    var obstacleRef = elevationCallObstacles[elevationCallIdx];
                    var obstaclePosition = obstacleLocations[obstacleRef.Idx];

                    lookAtPositionRef.Value = (true, obstaclePosition);
                }

                return true;
            }
            else
            {
                var cost = Vector3.Distance(elevationCallInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
            }
        }
        else
        {
            var cost = 0f;

            impactingBeliefs.Add(cost, BeliefEnum.WithinElevatorCallLocation, elevationCallIdx);
        }

        return false;
    }
}

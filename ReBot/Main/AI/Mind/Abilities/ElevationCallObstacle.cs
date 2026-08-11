using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct ElevationCallObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<ObstacleRef> elevationCallObstacles = ObstacleState.ElevationCallObstacles;
    NativeList<Vector3> elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
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
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];
        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

        var cost = Vector3.Distance(obstaclePosition, elevationCallInteractPosition) + 5f;

        impactingBeliefs.Add(cost, BeliefEnum.ElevatorCalled, elevationCallIdx);

        return false;
    }

    public bool Calls(int elevationCallIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];

        if (!obstaclesByTarget.TryGetValue(elevationCallInteractPosition, out var obstacle2Ref))
        {
            if (Vector3.Distance(elevationCallInteractPosition, playerPosition) < 2f)
            {
                lookAtPositionRef.Value = (true, elevationCallInteractPosition);
                interactPositionRef.Value = (true, elevationCallInteractPosition);
            }
            else
            {
                moveToPositionRef.Value = (true, elevationCallInteractPosition);
            }

            return true;
        }
        else
        {
            var cost = Vector3.Distance(elevationCallInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
        }

        return false;
    }
}

using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct ElevationTravelObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeList<ObstacleRef> elevationTravelObstacles = ObstacleState.ElevationTravelObstacles;
    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
    NativeList<Vector3> elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool TryGetElevationIdx(ObstacleRef obstacleRef, out int elevationIdx)
    {
        if (elevationTravelObstacles.IndexOf(obstacleRef) is var resultElevationIdx && resultElevationIdx > -1)
        {
            elevationIdx = resultElevationIdx;
            return true;
        }

        elevationIdx = default;
        return false;
    }

    public void Overcomes(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = 5f;

        impactingBeliefs.Add(cost, BeliefEnum.TravelingOnElevator, elevationIdx);
    }

    public bool Travels(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

        if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
        {
            var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
            interactPositionRef.Value = (true, travelInteractPosition);

            return true;
        }
        else
        {
            if (!obstaclesByTarget.TryGetValue(elevationEnterPosition, out var obstacle2Ref))
            {
                var cost = Vector3.Distance(elevationEnterPosition, playerPosition);

                impactingBeliefs.Add(cost, BeliefEnum.InElevator, elevationIdx);
            }
            else
            {
                var cost = Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
            }
        }

        return false;
    }

    public bool MovesToElevator(int elevationIdx)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];
        moveToPositionRef.Value = (true, elevationEnterPosition);

        return true;
    }
}

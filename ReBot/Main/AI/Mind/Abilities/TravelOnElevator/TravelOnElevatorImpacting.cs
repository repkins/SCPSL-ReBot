using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.TravelOnElevator;

internal struct TravelOnElevatorImpacting()
{
    NativeList<ObstacleRef> elevationTravelObstacles = ObstacleState.ElevationTravelObstacles;

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

    public bool Process(int elevationIdx, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;

        impactingActions.Add(cost, ActionEnum.TravelsOn | ActionEnum.Elevator, elevationIdx);

        return false;
    }
}

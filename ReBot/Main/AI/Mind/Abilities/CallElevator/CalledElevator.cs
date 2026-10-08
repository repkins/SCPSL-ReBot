using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.CallElevator;

internal struct CalledElevator()
{
    NativeList<ObstacleRef> elevationCallObstacles = ObstacleState.ElevationCallObstacles;

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

    public bool Process(int elevationCallIdx, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;

        impactingActions.Add(cost, ActionEnum.Calls | ActionEnum.Elevator, elevationCallIdx);

        return false;
    }
}

using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.OvercomeObstacle;

internal struct OvercomeObstacleEnabling()
{
    TravelOnElevatorImpacting travelOnElevatorImpacting = new();
    CallElevatorImpacting callElevatorImpacting = new();
    InteractObstacleImpacting interactObstacleImpacting = new();

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        return travelOnElevatorImpacting.TryGetElevationIdx(new ObstacleRef(targetLocationIdx), out var elevationIdx)
            ? travelOnElevatorImpacting.Process(elevationIdx, impactingActions)
            : callElevatorImpacting.TryGetElevationCallIdx(new ObstacleRef(targetLocationIdx), out var elevationCallIdx)
                ? callElevatorImpacting.Process(elevationCallIdx, impactingActions)
                : interactObstacleImpacting.Process(new(targetLocationIdx), impactingActions);
    }
}

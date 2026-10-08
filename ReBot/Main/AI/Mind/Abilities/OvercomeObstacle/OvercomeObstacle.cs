using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.OvercomeObstacle;

internal struct OvercomeObstacle()
{
    TraveledOnElevator traveledOnElevator = new();
    CalledElevator calledElevator = new();
    InteractedObstacle interactedObstacle = new();

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        return traveledOnElevator.TryGetElevationIdx(new ObstacleRef(targetLocationIdx), out var elevationIdx)
            ? traveledOnElevator.Process(elevationIdx, impactingActions)
            : calledElevator.TryGetElevationCallIdx(new ObstacleRef(targetLocationIdx), out var elevationCallIdx)
                ? calledElevator.Process(elevationCallIdx, impactingActions)
                : interactedObstacle.Process(new(targetLocationIdx), impactingActions);
    }
}

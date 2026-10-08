using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.Passthrough;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.OvercomeObstacle.Scp106;

internal struct OvercomeObstacle(BotRef botRef)
{
    TraveledOnElevator traveledOnElevator = new();
    CalledElevator calledElevator = new();
    InteractedObstacle interactedObstacle = new();
    PassedThroughObstacle passedThroughObstacle = new(botRef);

    public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions)
    {
        return traveledOnElevator.TryGetElevationIdx(new ObstacleRef(targetLocationIdx), out var elevationIdx)
            ? traveledOnElevator.Process(elevationIdx, impactingActions)
            : calledElevator.TryGetElevationCallIdx(new ObstacleRef(targetLocationIdx), out var elevationCallIdx)
                ? calledElevator.Process(elevationCallIdx, impactingActions)
                : interactedObstacle.Process(new(targetLocationIdx), impactingActions)
                    | passedThroughObstacle.Process(new(targetLocationIdx), impactingActions);
    }
}

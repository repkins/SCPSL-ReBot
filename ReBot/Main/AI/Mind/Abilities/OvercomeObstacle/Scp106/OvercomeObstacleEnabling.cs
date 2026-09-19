using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.Passthrough;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.OvercomeObstacle.Scp106;

internal struct OvercomeObstacleEnabling(BotRef botRef)
{
    TravelOnElevatorImpacting travelOnElevatorImpacting = new();
    CallElevatorImpacting callElevatorImpacting = new();
    InteractObstacleImpacting interactObstacleImpacting = new();
    PassthroughObstacleImpacting passthroughObstacleImpacting = new(botRef);

    public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return travelOnElevatorImpacting.TryGetElevationIdx(new ObstacleRef(targetLocationIdx), out var elevationIdx)
            ? travelOnElevatorImpacting.Process(elevationIdx, impactingBeliefs)
            : callElevatorImpacting.TryGetElevationCallIdx(new ObstacleRef(targetLocationIdx), out var elevationCallIdx)
                ? callElevatorImpacting.Process(elevationCallIdx, impactingBeliefs)
                : interactObstacleImpacting.Process(new(targetLocationIdx), impactingBeliefs)
                    | passthroughObstacleImpacting.Process(new(targetLocationIdx), impactingBeliefs);
    }
}

using ReBot.Main.AI.Mind.Abilities.CallsElevator;
using ReBot.Main.AI.Mind.Abilities.TravelOn;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Overcome;

internal struct OvercomeEnablingObstacle(BotRef botRef)
{
    TravelOnImpactingElevator travelOnElevatorImpacting = new();
    CallImpactingElevator callElevatorImpacting = new();

    public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return travelOnElevatorImpacting.TryGetElevationIdx(new ObstacleRef(targetLocationIdx), out var elevationIdx)
            ? travelOnElevatorImpacting.Process(elevationIdx, impactingBeliefs)
            : callElevatorImpacting.TryGetElevationCallIdx(new ObstacleRef(targetLocationIdx), out var elevationCallIdx)
                ? callElevatorImpacting.Process(elevationCallIdx, impactingBeliefs)
                : interactableObstacle.Overcomes(new(targetLocationIdx), impactingBeliefs);
    }
}

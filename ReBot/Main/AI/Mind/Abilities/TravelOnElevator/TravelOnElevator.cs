using ReBot.Main.AI.Mind.Abilities.GoTo;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.TravelOnElevator;

internal struct TravelOnElevator(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<ObstacleRef> elevationTravelObstacles = ObstacleState.ElevationTravelObstacles;
    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
    NativeList<Vector3> elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

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

    public bool Impacting(int elevationIdx, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;

        impactingActions.Add(cost, ActionEnum.TravelsOn | ActionEnum.Elevator, elevationIdx);

        return false;
    }

    public bool Enabling(int elevationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

        if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
        {
            var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
            lookAtPositionRef.Value = (true, travelInteractPosition);
            interactPositionRef.Value = (true, travelInteractPosition);

            return true;
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinElevator, elevationIdx));
        }

        return false;
    }
}

using ReBot.Main.AI.Mind.Abilities.GoTo;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.TravelOnElevator;

internal struct TravelOnElevatorEnabling<TGoToImpacting>(BotRef botRef, TGoToImpacting goToElevatorImpacting)
    where TGoToImpacting : IGoToImpacting
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
    NativeList<Vector3> elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool Process(int elevationIdx, NativeList<ImpactingAction> impactingActions)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

        if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
        {
            if (!obstaclesByTarget.TryGetValue(elevationEnterPosition, out var obstacle2Ref))
            {
                var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
                lookAtPositionRef.Value = (true, travelInteractPosition);
                interactPositionRef.Value = (true, travelInteractPosition);

                return true;
            }
            else
            {
                var cost = Vector3.Distance(elevationEnterPosition, obstacleLocations[obstacle2Ref.Idx]);

                impactingActions.Add(cost, ActionEnum.Overcomes | ActionEnum.Obstacle, obstacle2Ref.Idx);
            }
        }
        else
        {
            goToElevatorImpacting.Process(elevationIdx, impactingActions);
        }

        return false;
    }
}

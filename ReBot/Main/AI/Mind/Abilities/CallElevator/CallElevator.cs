using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.CallElevator;

internal struct CallElevator(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<ObstacleRef> elevationCallObstacles = ObstacleState.ElevationCallObstacles;
    NativeList<Vector3> elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;
    NativeList<bool> elevationCallInteractAbles = ObstacleState.ElevationCallInteractAbles;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

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

    public void Impacting(int elevationCallIdx, NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;

        impactingActions.Add(cost, ActionEnum.Calls | ActionEnum.Elevator, elevationCallIdx);
    }

    public void Enabling(int elevationCallIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];
        var elevationCallInteractAble = elevationCallInteractAbles[elevationCallIdx];

        if (Vector3.Distance(elevationCallInteractPosition, playerPosition) < 2f)
        {
            if (elevationCallInteractAble)
            {
                lookAtPositionRef.Value = (true, elevationCallInteractPosition);
                interactPositionRef.Value = (true, elevationCallInteractPosition);
            }
            else
            {
                var obstacleRef = elevationCallObstacles[elevationCallIdx];
                var obstaclePosition = obstacleLocations[obstacleRef.Idx];

                lookAtPositionRef.Value = (true, obstaclePosition);
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinElevatorCall, elevationCallIdx));
        }
    }
}

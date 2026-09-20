using ReBot.Main.AI.Mind.Abilities.GoTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.CallElevator;

internal struct CallElevatorEnabling(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<ObstacleRef> elevationCallObstacles = ObstacleState.ElevationCallObstacles;
    NativeList<Vector3> elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations;
    NativeList<bool> elevationCallInteractAbles = ObstacleState.ElevationCallInteractAbles;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    GoTo<ElevatorCall>.Impacting goToElevatorCallImpacting = new(botRef, new());

    public bool Process(int elevationCallIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var elevationCallInteractPosition = elevationCallInteractLocations[elevationCallIdx];
        var elevationCallInteractAble = elevationCallInteractAbles[elevationCallIdx];

        if (Vector3.Distance(elevationCallInteractPosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(elevationCallInteractPosition, out var obstacle2Ref))
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

                return true;
            }
            else
            {
                var cost = Vector3.Distance(elevationCallInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                impactingBeliefs.Add(cost, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacle2Ref.Idx);
            }
        }
        else
        {
            goToElevatorCallImpacting.Process(elevationCallIdx, impactingBeliefs);
        }

        return false;
    }
}

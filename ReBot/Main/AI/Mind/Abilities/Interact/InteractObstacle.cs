using Interactables.Interobjects.DoorUtils;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Interact;

internal struct InteractObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<DoorPermissionFlags> doorObstaclePermissions = ObstacleState.DoorObstaclePermissions;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool Impacting(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        var doorPermissions = doorObstaclePermissions[obstacleRef.Idx];
        if (doorPermissions == DoorPermissionFlags.None)
        {
            var cost = 1f;

            impactingActions.Add(cost, ActionEnum.InteractsWith | ActionEnum.Obstacle, obstacleRef.Idx);
        }

        return false;
    }

    public bool Enabling(int obstacleIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var obstacleInteractPosition = obstacleLocations[obstacleIdx];

        if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
            {
                interactPositionRef.Value = (true, obstacleInteractPosition);
                lookAtPositionRef.Value = (true, obstacleInteractPosition);

                return true;
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.ObstructedObstacle, obstacle2Ref.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinObstacle, obstacleIdx));
        }

        return false;
    }
}

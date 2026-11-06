using Interactables.Interobjects.DoorUtils;
using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Interact;

internal struct InteractObstacle(BotRef botRef, DoorPermissionFlags rolePermissions)
{
    Vector3 playerPosition = botRef.Value.PlayerStore.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<DoorPermissionFlags> doorObstaclePermissions = ObstacleState.DoorObstaclePermissions;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleStore.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionStore.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementStore.LookAtPositionRef;

    public void Impacting(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        var doorPermissions = doorObstaclePermissions[obstacleRef.Idx];
        if (doorPermissions == DoorPermissionFlags.None || doorPermissions.HasFlagAny(rolePermissions))
        {
            var cost = 1f;

            impactingActions.Add(cost, ActionEnum.InteractsWith | ActionEnum.Obstacle, obstacleRef.Idx);
        }
    }

    public void Enabling(int obstacleIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var obstacleInteractPosition = obstacleLocations[obstacleIdx];

        if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
            {
                interactPositionRef.Value = (true, obstacleInteractPosition);
                lookAtPositionRef.Value = (true, obstacleInteractPosition);
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacle2Ref.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinObstacle, obstacleIdx));
        }
    }
}

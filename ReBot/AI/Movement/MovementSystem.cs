using ReBot.AI.Movement.Pathfinding;
using UnityEngine;

namespace ReBot.AI.Movement;

public static class MovementSystem
{
    private const float MaxSteeringForceDegrees = 640f;

    public static void Process()
    {
        MovementStore.PlayerTransforms.Clear();
        MovementStore.CameraTransforms.Clear();
        MovementStore.CameraPositions.Clear();
        MovementStore.PlayerPositions.Clear();

        foreach (var (botRef, fpcRole) in MovementStore.CurrentFpcPlayerRoles)
        {
            var referenceHub = BotStore.BotHubs[botRef.Idx].ReferenceHub;

            MovementStore.PlayerTransforms.Add(fpcRole.FpcModule.transform);
            MovementStore.CameraTransforms.Add(referenceHub.PlayerCameraReference);

            MovementStore.CameraPositions.Add(fpcRole.CameraPosition);
            MovementStore.PlayerPositions.Add(fpcRole.FpcModule.Position);
        }

        PathfindingSystem.Process();

        MovementStore.DesiredMoves.Clear();
        MovementStore.DesiredLooks.Clear();

        foreach (var (botFpcRoleRef, position) in PathfindingStore.ClosestPositions)
        {
            SteerToPosition(botFpcRoleRef, position);
        }
    }

    private static void SteerToPosition(FpcRoleRef botFpcRoleRef, Vector3 targetPosition)
    {
        var relativePos = targetPosition - MovementStore.CameraPositions[botFpcRoleRef.Idx];
        var relativeHorizontalPos = Vector3.ProjectOnPlane(relativePos, Vector3.up);
        var turnPosition = relativeHorizontalPos + MovementStore.CameraPositions[botFpcRoleRef.Idx];

        ToPosition(botFpcRoleRef, turnPosition);

        var playerDirection = MovementStore.PlayerTransforms[botFpcRoleRef.Idx].forward;
        var dirTowardsTarget = Vector3.Normalize(relativeHorizontalPos);

        if (Vector3.Dot(playerDirection, dirTowardsTarget) < 0f)
        {
            MovementStore.DesiredMoves.Add(new(MovementStore.PlayerTransforms[botFpcRoleRef.Idx].InverseTransformDirection(dirTowardsTarget)));
        }
        else
        {
            MovementStore.DesiredMoves.Add(new(Vector3.forward));
        }
    }

    private static void ToPosition(FpcRoleRef botFpcRoleRef, Vector3 targetPosition)
    {
        var playerTransform = MovementStore.PlayerTransforms[botFpcRoleRef.Idx];
        var cameraTransform = MovementStore.CameraTransforms[botFpcRoleRef.Idx];

        var relativePos = targetPosition - cameraTransform.position;

        var hDirectionToTarget = Vector3.ProjectOnPlane(relativePos, Vector3.up).normalized;
        var hForward = playerTransform.forward;

        var hRotation = Quaternion.FromToRotation(hForward, hDirectionToTarget);

        var hReverseRotation = Quaternion.Inverse(hRotation);

        var vDirectionToTarget = Vector3.Normalize(hReverseRotation * relativePos);
        var vForward = cameraTransform.forward;
        var vLocalForward = playerTransform.InverseTransformDirection(vForward);
        var vLocalDirToTarget = playerTransform.InverseTransformDirection(vDirectionToTarget);

        var vRotation = Quaternion.FromToRotation(vLocalForward, vLocalDirToTarget);

        var targetHRotation = hRotation;

        hRotation = Quaternion.Slerp(Quaternion.identity, hRotation, .075f);
        vRotation = Quaternion.Slerp(Quaternion.identity, vRotation, .075f);

        hRotation = Quaternion.RotateTowards(Quaternion.identity, hRotation, Time.deltaTime * MaxSteeringForceDegrees);
        vRotation = Quaternion.RotateTowards(Quaternion.identity, vRotation, Time.deltaTime * MaxSteeringForceDegrees);

        var desiredLook = new Look(hRotation, vRotation, targetHRotation);

        MovementStore.DesiredLooks.Add(desiredLook);
    }
}

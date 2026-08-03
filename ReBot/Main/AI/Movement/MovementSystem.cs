using PlayerRoles.FirstPersonControl;
using ReBot.Main.AI.Movement.Pathfinding;
using UnityEngine;

namespace ReBot.Main.AI.Movement;

public static class MovementSystem
{
    private const float MaxSteeringForceDegrees = 640f;

    public static void Process()
    {
        PathfindingSystem.Process();

        foreach (var (botRef, position) in PathfindingStore.ClosestPositions)
        {
            SteerToPosition(botRef, position);
        }
    }

    private static void SteerToPosition(BotRef botRef, Vector3 targetPosition)
    {
        var bot = BotStore.Bots[botRef.Idx];

        var relativePos = targetPosition - bot.PlayerState.CameraPosition;
        var relativeHorizontalPos = Vector3.ProjectOnPlane(relativePos, Vector3.up);
        var turnPosition = relativeHorizontalPos + bot.PlayerState.CameraPosition;

        ToPosition(botRef, turnPosition);

        var playerDirection = bot.PlayerState.Forward;
        var dirTowardsTarget = Vector3.Normalize(relativeHorizontalPos);

        if (Vector3.Dot(playerDirection, dirTowardsTarget) < 0f)
        {
            bot.MovementState.DesiredMove = new(bot.PlayerTransform.InverseTransformDirection(dirTowardsTarget));
        }
        else
        {
            bot.MovementState.DesiredMove = new(Vector3.forward);
        }
    }

    private static void ToPosition(BotRef botRef, Vector3 targetPosition)
    {
        var bot = BotStore.Bots[botRef.Idx];

        var playerTransform = bot.PlayerTransform;
        var cameraTransform = bot.CameraTransform;

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

        bot.MovementState.DesiredLook = desiredLook;
    }
}

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp173;
using ReBot.Main.AI.Movement.Pathfinding;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Blink;

internal class BlinkSystem
{
    static readonly MethodInfo m_Scp173TeleportAbility_UpdateAiming = AccessTools.Method(typeof(Scp173TeleportAbility), "UpdateAiming", [typeof(bool)]);

    public static void Process()
    {
        PathfindingStore.DestinationPositions.Clear();
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var blinkToPositionRef = BotStore.Bots[botIdx].BlinkState.BlinkToPositionRef;

            var (hasPosition, position) = blinkToPositionRef.Value;
            blinkToPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            PathfindingStore.DestinationPositions.Add((new(botIdx), position));
        }

        var closestPositions = new NativeList<(BotRef, Vector3)>(Allocator.Temp);
        PathfindingSystem.Process(closestPositions);

        foreach (var (botRef, closestPosition) in closestPositions)
        {
            var botIdx = botRef.Idx;
            var hub = botRef.Value.Hub;

            if (hub.roleManager.CurrentRole is not Scp173Role scp173Role)
            {
                DebugLog.LogWarning($"Role is not SCP 173 but has set blink to position for player {hub}");
                continue;
            }

            if (!scp173Role.SubroutineModule.TryGetSubroutine<Scp173TeleportAbility>(out var teleportAbility))
            {
                DebugLog.LogWarning($"Role is SCP 173 but has none it's own teleport ablility for player {hub}");
                continue;
            }

            var playerCameraPosition = BotStore.Bots[botIdx].PlayerState.CameraPosition;
            var playerCameraForward = BotStore.Bots[botIdx].PlayerState.CameraForward;

            var position = closestPosition;
            if (PathfindingStore.GoalPositions[botIdx] != closestPosition)
            {
                var playerForward = BotStore.Bots[botIdx].PlayerState.Forward;
                position += playerForward * 0.5f;
            }

            if (!Physics.Raycast(position, Vector3.down, out var hitInfo, 2f, LayerMask.GetMask("Default")))
            {
                continue;
            }
            var tpPosition = hitInfo.point;

            var relTargetPos = tpPosition - playerCameraPosition;
            if (relTargetPos.magnitude - Vector3.Dot(relTargetPos, playerCameraForward) < .1f)
            {
                m_Scp173TeleportAbility_UpdateAiming.Invoke(teleportAbility, [false]);
                m_Scp173TeleportAbility_UpdateAiming.Invoke(teleportAbility, [true]);
            }

            BotStore.Bots[botIdx].MovementState.LookAtPositionRef.Value = (true, tpPosition);
        }
    }
}

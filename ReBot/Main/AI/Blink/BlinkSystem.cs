using ReBot.Main.AI.Movement.Pathfinding;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Blink;

internal class BlinkSystem
{
    static readonly string BlinkDummyActionName = $"{ActionName.Zoom}->Click";

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

            var playerCameraPosition = BotStore.Bots[botIdx].PlayerState.CameraPosition;
            var playerCameraForward = BotStore.Bots[botIdx].PlayerState.CameraForward;

            var targetPosition = closestPosition;
            if (PathfindingStore.GoalPositions[botIdx] != closestPosition)
            {
                targetPosition = closestPosition;
            }
            else
            {
                if (!Physics.Raycast(targetPosition, Vector3.down, out var hitInfo, 2f, LayerMask.GetMask("Default")))
                {
                    continue;
                }
                targetPosition = hitInfo.point;
            }

            var relTargetPos = targetPosition - playerCameraPosition;
            if (relTargetPos.magnitude - Vector3.Dot(relTargetPos, playerCameraForward) < .01f)
            {
                BotStore.Bots[botIdx].ActionState.WantSet.Add(BlinkDummyActionName);
            }

            BotStore.Bots[botIdx].MovementState.LookAtPositionRef.Value = (true, targetPosition);
        }
    }
}

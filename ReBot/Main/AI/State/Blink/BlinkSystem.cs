using ReBot.Main.AI.State.Movement.Pathfinding;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Blink;

internal class BlinkSystem
{
    static readonly string BlinkDummyActionName = $"{ActionName.Zoom}->Click";

    public static void Process()
    {
        PathfindingStore.DestinationPositions.Clear();
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var blinkToPositionRef = BotStore.Bots[botIdx].BlinkStore.BlinkToPositionRef;

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

            var playerCameraPosition = BotStore.Bots[botIdx].PlayerStore.CameraPosition;
            var playerCameraForward = BotStore.Bots[botIdx].PlayerStore.CameraForward;

            var blinkPosition = closestPosition + playerCameraForward * 0.1f;
            if (PathfindingStore.GoalPositions[botIdx] == closestPosition)
            {
                if (Physics.Raycast(blinkPosition, Vector3.down, out var hitInfo, 2f, LayerMask.GetMask("Default")))
                {
                    blinkPosition = hitInfo.point;
                }
            }

            ref var blinkState = ref BotStore.Bots[botIdx].BlinkStore;

            if (playerCameraPosition != blinkState.LastCameraPosition)
            {
                blinkState.DownAimAmount = 0f;
            }
            blinkState.LastCameraPosition = playerCameraPosition;

            blinkPosition += Vector3.down * blinkState.DownAimAmount;

            BotStore.Bots[botIdx].MovementStore.LookAtPositionRef.Value = (true, blinkPosition);

            if (blinkState.RemainingSecondsToBlink > .01f)
            {
                continue;
            }

            var relTargetPos = blinkPosition - playerCameraPosition;
            if (relTargetPos.magnitude - Vector3.Dot(relTargetPos, playerCameraForward) >= .01f)
            {
                continue;
            }

            var isHeld = blinkState.IsHeld;
            var prevIsHeld = isHeld;

            isHeld = !isHeld;

            blinkState.IsHeld = isHeld;

            if (prevIsHeld != isHeld)
            {
                if (isHeld)
                {
                    BotStore.Bots[botIdx].ActionStore.WantSet.Add(BlinkDummyActionName);

                    blinkState.DownAimAmount += .1f;
                }
            }
        }
    }
}

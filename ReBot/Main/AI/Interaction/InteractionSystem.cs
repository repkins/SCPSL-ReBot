using Interactables;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Interaction;

internal static class InteractionSystem
{
    private const float MaxInteractDistance = 2f;

    private static readonly int RaycastLayerMask = LayerMask.GetMask("Door", "InteractableNoPlayerCollision", "Default");

    public static void Process()
    {
        var raycastBotRefs = new NativeList<BotRef>(BotStore.Bots.Count, Allocator.Temp);
        var raycastCommands = new NativeList<RaycastCommand>(BotStore.Bots.Count, Allocator.Temp);

        var raycastHits = new NativeArray<RaycastHit>(BotStore.Bots.Count, Allocator.Temp);

        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++) 
        {
            var interactWithPositionRef = BotStore.Bots[botIdx].InteractionState.InteractWithPositionRef;

            var (hasPosition, _) = interactWithPositionRef.Value;
            interactWithPositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var playerCameraPosition = BotStore.Bots[botIdx].PlayerState.CameraPosition;
            var playerCameraForward = BotStore.Bots[botIdx].PlayerState.CameraForward;

            var raycastCommand = new RaycastCommand(playerCameraPosition, playerCameraForward, new(RaycastLayerMask), MaxInteractDistance);

            raycastCommands.Add(raycastCommand);
            raycastBotRefs.Add(new(botIdx));
        }

        var raycastCommandHandle = RaycastCommand.ScheduleBatch(raycastCommands.AsArray(), raycastHits, 1);
        raycastCommandHandle.Complete();

        for (var raycastIdx = 0; raycastIdx < raycastHits.Length; raycastIdx++)
        {
            var hit = raycastHits[raycastIdx];

            if (hit.collider is null 
                || hit.collider.GetComponent<InteractableCollider>() is not InteractableCollider interactableCollider
                || interactableCollider.Target is not IServerInteractable interactable)
            {
                continue;
            }

            var botRef = raycastBotRefs[raycastIdx];
            var hub = BotStore.Bots[botRef.Idx].Hub;
            var colliderId = interactableCollider.ColliderId;
            interactable.ServerInteract(hub, colliderId);
        }
    }
}

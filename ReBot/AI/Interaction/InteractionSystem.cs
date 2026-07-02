using Interactables;
using ReBot.AI.Spacial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Interaction;

internal static class InteractionSystem
{
    private const float MaxInteractDistance = 2f;

    private static readonly int RaycastLayerMask = LayerMask.GetMask("Door", "Glass");

    public static void Process()
    {
        var raycastBotRefs = new NativeList<BotRef>(InteractionStore.InteractWithPositionRefs.Count, Allocator.Temp);
        var raycastCommands = new NativeList<RaycastCommand>(InteractionStore.InteractWithPositionRefs.Count, Allocator.Temp);

        var raycastHits = new NativeArray<RaycastHit>(InteractionStore.InteractWithPositionRefs.Count, Allocator.Temp);

        for (var botIdx = 0; botIdx < InteractionStore.InteractWithPositionRefs.Count; botIdx++) 
        {
            var (hasPosition, position) = InteractionStore.InteractWithPositionRefs[botIdx].Value;
            if (!hasPosition)
            {
                continue;
            }

            var playerCameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var playerCameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            var raycastCommand = new RaycastCommand(playerCameraPosition, playerCameraForward, new(RaycastLayerMask), MaxInteractDistance);

            raycastCommands.Add(raycastCommand);
            raycastBotRefs.Add(new(botIdx));
        }

        var raycastCommandHandle = RaycastCommand.ScheduleBatch(raycastCommands.AsArray(), raycastHits, 1);

        raycastCommandHandle.Complete();

        for (var raycastIdx = 0; raycastIdx < raycastCommands.Length; raycastIdx++)
        {
            var hit = raycastHits[raycastIdx];

            if (hit.collider is null 
                || hit.collider.GetComponent<InteractableCollider>() is not InteractableCollider interactableCollider
                || interactableCollider.Target is not IServerInteractable interactable)
            {
                continue;
            }

            var botRef = raycastBotRefs[raycastIdx];
            var hub = BotStore.BotHubs[botRef.Idx];
            var colliderId = interactableCollider.ColliderId;
            interactable.ServerInteract(hub, colliderId);
        }
    }
}

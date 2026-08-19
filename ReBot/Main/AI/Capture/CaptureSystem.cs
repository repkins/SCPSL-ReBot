using HarmonyLib;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Subroutines;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Capture;

internal class CaptureSystem
{
    static readonly LayerMask RaycastLayerMask = LayerMask.GetMask("Hitbox");
    const float AttackMaxDistance = 1.5f;

    static readonly FieldInfo f_scp106Attack_claimedOwnerPosition = AccessTools.Field(typeof(Scp106Attack), "_claimedOwnerPosition");
    static readonly FieldInfo f_scp106Attack_claimedOwnerCamRotation = AccessTools.Field(typeof(Scp106Attack), "_claimedOwnerCamRotation");
    static readonly FieldInfo f_scp106Attack_claimedTargetPosition = AccessTools.Field(typeof(Scp106Attack), "_claimedTargetPosition");
    static readonly FieldInfo f_scp106Attack_targetHub = AccessTools.Field(typeof(Scp106Attack), "_targetHub");

    public static void Process()
    {
        var raycastBotRefs = new NativeList<BotRef>(BotStore.Bots.Count, Allocator.Temp);
        var raycastCommands = new NativeList<RaycastCommand>(BotStore.Bots.Count, Allocator.Temp);

        var raycastHits = new NativeArray<RaycastHit>(BotStore.Bots.Count, Allocator.Temp);

        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var capturePositionRef = BotStore.Bots[botIdx].CaptureState.CapturePositionRef;

            var (hasPosition, _) = capturePositionRef.Value;
            capturePositionRef.Value = default;

            if (!hasPosition)
            {
                continue;
            }

            var playerCameraPosition = BotStore.Bots[botIdx].PlayerState.CameraPosition;
            var playerCameraForward = BotStore.Bots[botIdx].PlayerState.CameraForward;

            var raycastCommand = new RaycastCommand(playerCameraPosition, playerCameraForward, new(RaycastLayerMask), AttackMaxDistance);

            raycastCommands.Add(raycastCommand);
            raycastBotRefs.Add(new(botIdx));
        }

        var raycastCommandHandle = RaycastCommand.ScheduleBatch(raycastCommands.AsArray(), raycastHits, 1);
        raycastCommandHandle.Complete();

        for (var raycastIdx = 0; raycastIdx < raycastHits.Length; raycastIdx++)
        {
            var hit = raycastHits[raycastIdx];

            if (hit.collider is null
                || !hit.collider.TryGetComponent<HitboxIdentity>(out var hitboxIdentity))
            {
                continue;
            }

            var targetHub = hitboxIdentity.TargetHub;
            var botRef = raycastBotRefs[raycastIdx];
            var hub = BotStore.Bots[botRef.Idx].Hub;

            if (hub.roleManager.CurrentRole is not Scp106Role scp106Role)
            {
                DebugLog.LogWarning($"Role is not SCP 106 but has set capture position for player {hub}");
                continue;
            }

            if (!scp106Role.SubroutineModule.TryGetSubroutine<Scp106Attack>(out var attackAbility))
            {
                DebugLog.LogWarning($"Role is SCP 106 but has no capture ability for player {hub}");
                continue;
            }

            var claimedOwnerPosition = BotStore.Bots[botRef.Idx].PlayerState.Position;
            var claimedOwnerCamRotation = BotStore.Bots[botRef.Idx].CameraTransform.rotation;
            var claimedTargetPosition = hit.point;

            f_scp106Attack_claimedOwnerPosition.SetValue(attackAbility, claimedOwnerPosition);
            f_scp106Attack_claimedOwnerCamRotation.SetValue(attackAbility, claimedOwnerCamRotation);
            f_scp106Attack_claimedTargetPosition.SetValue(attackAbility, claimedTargetPosition);
            f_scp106Attack_targetHub.SetValue(attackAbility, targetHub);

            var connectionToServer = BotStore.Bots[botRef.Idx].ConnectionToServer;
            connectionToServer.Send<SubroutineMessage>(new(attackAbility, false));
        }
    }
}

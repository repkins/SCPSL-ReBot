using HarmonyLib;
using Interactables;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Subroutines;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Cardiac;

internal class CardiacSystem
{
    static readonly LayerMask RaycastLayerMask = LayerMask.GetMask("Hitbox");
    const float AttackMaxDistance = 1.728f;

    static readonly FieldInfo f_scp049AttackAbility_target = AccessTools.Field(typeof(Scp049AttackAbility), "_target");

    public static void Process()
    {
        var raycastBotRefs = new NativeList<BotRef>(BotStore.Bots.Count, Allocator.Temp);
        var raycastCommands = new NativeList<RaycastCommand>(BotStore.Bots.Count, Allocator.Temp);

        var raycastHits = new NativeArray<RaycastHit>(BotStore.Bots.Count, Allocator.Temp);

        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var cardiacPositionRef = BotStore.Bots[botIdx].CardiacState.CardiacPositionRef;

            var (hasPosition, _) = cardiacPositionRef.Value;
            cardiacPositionRef.Value = default;

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

            if (hub.roleManager.CurrentRole is not Scp049Role scp049Role)
            {
                DebugLog.LogWarning($"Role is not SCP 049 but has set cardiac position for player {hub}");
                continue;
            }

            if (!scp049Role.SubroutineModule.TryGetSubroutine<Scp049AttackAbility>(out var attackAbility))
            {
                DebugLog.LogWarning($"Role is SCP 049 but has no attack ability for player {hub}");
                continue;
            }

            f_scp049AttackAbility_target.SetValue(attackAbility, targetHub);

            var connectionToServer = BotStore.Bots[botRef.Idx].ConnectionToServer;
            connectionToServer.Send<SubroutineMessage>(new(attackAbility, false));
        }
    }
}

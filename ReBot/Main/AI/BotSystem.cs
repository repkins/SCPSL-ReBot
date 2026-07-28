using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using MEC;
using PlayerRoles;
using ReBot.Main.AI.Cardiac;
using ReBot.Main.AI.Interaction;
using ReBot.Main.AI.Mind;
using ReBot.Main.AI.Movement;
using ReBot.Main.AI.Perception;
using ReBot.Main.AI.Spacial;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.Main.AI;

public static class BotSystem
{
    private static CoroutineHandle handle;

    public static void Initalize()
    {
        handle = Timing.RunCoroutine(RunPlayerUpdates());

        PlayerRoleManager.OnRoleChanged += OnRoleChanged;
        ReferenceHub.OnPlayerRemoved += RemovePlayerIfBot;
        ServerEvents.RoundRestarted += OnRoundRestarted;
        PlayerEvents.Kicked += PrintKickReasonIfBot;
        ServerEvents.RoundStarted += AssignSupportedRole;

        PerceptionSystem.Initialize();
    }

    public static void Teardown()
    {
        PerceptionSystem.Teardown();

        PlayerRoleManager.OnRoleChanged -= OnRoleChanged;
        ReferenceHub.OnPlayerRemoved -= RemovePlayerIfBot;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
        PlayerEvents.Kicked -= PrintKickReasonIfBot;
        ServerEvents.RoundStarted -= AssignSupportedRole;

        Timing.KillCoroutines(handle);
    }

    private static void OnRoundRestarted()
    {

    }

    private static void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        Debug.Log($"Bot \"{userHub}\" got new role {newRole.RoleTypeId}");
    }

    private static void RemovePlayerIfBot(ReferenceHub userHub)
    {
        if (BotStore.BotHubRefs.TryGetValue(userHub, out var botRef))
        {
            BotStore.RemoveBotPlayer(botRef);

            Debug.Log($"Bot player removed: {userHub}");
        }
    }

    private static void PrintKickReasonIfBot(PlayerKickedEventArgs args)
    {
        var hub = args.Player.ReferenceHub;
        if (BotStore.BotHubRefs.ContainsKey(hub))
        {
            Debug.Log($"Bot player \"{hub}\" got kicked, reason: {args.Reason}");
        }
    }

    private static void AssignSupportedRole()
    {
        for (int botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var hub = BotStore.Bots[botIdx].Hub;
            if (hub.roleManager.CurrentRole.RoleTypeId != RoleTypeId.Scp049)
            {
                hub.roleManager.ServerSetRole(RoleTypeId.Scp049, RoleChangeReason.None);
            }
        }
    }


    public static IEnumerator<float> RunPlayerUpdates()
    {
        while (true)
        {
            // Perception system
            PerceptionSystem.Process();

            DecisionMakingSystem.Process();

            MovementSystem.Process();
            InteractionSystem.Process();

            // SCP 049 Role
            CardiacSystem.Process();

            yield return Timing.WaitForOneFrame;
        }
    }
}
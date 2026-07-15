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
    }

    public static void Teardown()
    {
        Timing.KillCoroutines(handle);

        PlayerRoleManager.OnRoleChanged -= OnRoleChanged;
        ReferenceHub.OnPlayerRemoved -= RemovePlayerIfBot;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
    }

    private static void OnRoundRestarted()
    {

    }

    private static void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {

    }

    private static void RemovePlayerIfBot(ReferenceHub userHub)
    {
        if (BotStore.BotHubRefs.TryGetValue(userHub, out var botRef))
        {
            BotStore.RemoveBotPlayer(botRef);

            Debug.Log($"Bot player removed: {userHub}");
        }
    }

    public static IEnumerator<float> RunPlayerUpdates()
    {
        // Perception systems
        SightPerceptionSystem.Process();
        RoomPerceptionSystem.Process();

        DecisionMakingSystem.Process();

        MovementSystem.Process();
        InteractionSystem.Process();

        // SCP 049 Role
        CardiacSystem.Process();

        yield break;
    }
}


using LabApi.Events.Handlers;
using MEC;
using PlayerRoles;
using ReBot.AI.Movement;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static PlayerRoles.PlayerRoleManager;

namespace ReBot.AI;

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

    }

    public static IEnumerator<float> RunPlayerUpdates()
    {
        MovementSystem.Process();

        yield break;
    }
}
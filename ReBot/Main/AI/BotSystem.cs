using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using MEC;
using PlayerRoles;
using ReBot.Main.AI.Capture;
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

        PerceptionSystem.Initialize();
    }

    public static void Teardown()
    {
        PerceptionSystem.Teardown();

        Timing.KillCoroutines(handle);
    }

    public static void OnRoundRestarted()
    {

    }

    public static void PrintRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        Debug.Log($"Bot \"{userHub}\" got new role {newRole.RoleTypeId}");
    }

    public static void RemoveBotPlayer(ReferenceHub userHub, BotRef botRef)
    {
        BotStore.RemoveBotPlayer(botRef);

        Debug.Log($"Bot player removed: {userHub}");
    }

    public static void PrintKickReason(ReferenceHub hub, string reason)
    {
        Debug.Log($"Bot player \"{hub}\" got kicked, reason: {reason}");
    }

    public static void AssignSupportedRole()
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
            PerceptionSystem.Process();

            DecisionMakingSystem.Process();

            MovementSystem.Process();
            InteractionSystem.Process();

            CardiacSystem.Process(); // SCP 049 Role
            CaptureSystem.Process(); // SCP 106 Role

            yield return Timing.WaitForOneFrame;
        }
    }
}
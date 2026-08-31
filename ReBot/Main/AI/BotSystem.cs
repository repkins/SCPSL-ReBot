using MEC;
using PlayerRoles;
using ReBot.Main.AI.Blink;
using ReBot.Main.AI.Capture;
using ReBot.Main.AI.Cardiac;
using ReBot.Main.AI.Interaction;
using ReBot.Main.AI.Melee.Scp096;
using ReBot.Main.AI.Mind;
using ReBot.Main.AI.Movement;
using ReBot.Main.AI.NeckSnap;
using ReBot.Main.AI.Perception;
using ReBot.Main.AI.Rage;
using System;
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
            if (hub.roleManager.CurrentRole.RoleTypeId != RoleTypeId.Scp939)
            {
                hub.roleManager.ServerSetRole(RoleTypeId.Scp939, RoleChangeReason.None);
            }
        }
    }


    public static IEnumerator<float> RunPlayerUpdates()
    {
        while (true)
        {
            try
            {
                PerceptionSystem.Process();

                DecisionMakingSystem.Process();

                CardiacSystem.Process(); // SCP 049 Role
                CaptureSystem.Process(); // SCP 106 Role
                NeckSnapSystem.Process(); // SCP 173 Role
                BlinkSystem.Process(); // SCP 173 Role
                Scp096MeleeAttackSystem.Process(); // SCP 096 Role
                RageSystem.Process();

                MovementSystem.Process();
                InteractionSystem.Process();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Break();

                Debug.LogException(e);
                break;
            }

            yield return Timing.WaitForOneFrame;
        }
    }
}
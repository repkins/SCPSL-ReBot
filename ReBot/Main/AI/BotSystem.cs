using MEC;
using PlayerRoles;
using ReBot.Main.AI.Decision;
using ReBot.Main.AI.State;
using ReBot.Main.AI.State.Blink;
using ReBot.Main.AI.State.Capture;
using ReBot.Main.AI.State.Cardiac;
using ReBot.Main.AI.State.Interaction;
using ReBot.Main.AI.State.Melee;
using ReBot.Main.AI.State.Movement;
using ReBot.Main.AI.State.Rage;
using ReBot.Main.AI.State.Snap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.Main.AI;

internal static class BotSystem
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

    public static void PrintRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        Debug.Log($"Bot \"{userHub}\" got new role {newRole.RoleTypeId}");
    }

    public static void AddBotPlayer(ReferenceHub hub)
    {
        BotStore.AddBotPlayer(hub);

        Debug.Log($"Bot player added: {hub}");
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
                SnapSystem.Process(); // SCP 173 Role
                BlinkSystem.Process(); // SCP 173 Role
                RageActionSystem.Process(); // SCP 096 Role
                MeleeAttackSystem.Process(); // SCP 096 & SCP 939 Role

                MovementSystem.Process();
                InteractionSystem.Process();

                ActionSystem.Procees();
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
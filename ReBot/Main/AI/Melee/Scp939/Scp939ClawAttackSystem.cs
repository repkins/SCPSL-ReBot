using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;
using System.Reflection;

namespace ReBot.Main.AI.Melee.Scp939;

internal class Scp939ClawAttackSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var meleeAttackPositionRef = BotStore.Bots[botIdx].Scp939MeleeState.MeleeAttackPositionRef;

            var (hasPosition, _) = meleeAttackPositionRef.Value;
            meleeAttackPositionRef.Value = default;

            if (!hasPosition)
            {
                BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Shoot] = false;
                continue;
            }

            BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Shoot] = true;
        }
    }
}

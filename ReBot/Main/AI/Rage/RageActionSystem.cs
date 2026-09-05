using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;
using System.Reflection;

namespace ReBot.Main.AI.Rage;

internal class RageActionSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var enrageRef = BotStore.Bots[botIdx].RageState.EnrageRef;
            var wantsEnrage = enrageRef.Value;
            enrageRef.Value = default;

            var calmRef = BotStore.Bots[botIdx].RageState.CalmRef;
            var wantsCalm = calmRef.Value;
            calmRef.Value = default;

            if (!wantsEnrage && !wantsCalm)
            {
                BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Reload] = false;
                continue;
            }

            BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Reload] = true;
        }
    }
}

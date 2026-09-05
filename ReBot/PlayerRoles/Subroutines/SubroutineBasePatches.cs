using PlayerRoles.Subroutines;
using HarmonyLib;
using ReBot.Main.AI;

namespace ReBot.PlayerRoles.Subroutines;

[HarmonyPatch(typeof(SubroutineBase))]
internal static class SubroutineBasePatches
{
    [HarmonyPatch(nameof(SubroutineBase.GetAction))]
    [HarmonyPrefix()]
    public static bool SetResultIfBot(SubroutineBase __instance, ActionName action, ref bool __result)
    {
        if (__instance.Role.TryGetOwner(out var hub) 
            && BotStore.BotHubRefs.TryGetValue(hub, out var botRef)
            && botRef.Value.ActionState.IsHeld[action])
        {
            __result = true;
            botRef.Value.ActionState.IsHeld[action] = false;

            return false;
        }

        return true;
    }
}

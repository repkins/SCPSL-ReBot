using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Subroutines;
using ReBot.Main.AI;

namespace ReBot.PlayerRoles.Subroutines;

[HarmonyPatch(typeof(KeySubroutine<Scp939Role>))]
internal static class KeySubroutinePatches
{
    [HarmonyPatch("KeyPressable", MethodType.Getter)]
    [HarmonyPrefix()]
    public static bool SetResultTrueIfBot(KeySubroutine<Scp939Role> __instance, ref bool __result)
    {
        if (__instance.Role.TryGetOwner(out var hub)
            && BotStore.BotHubRefs.ContainsKey(hub))
        {
            __result = true;

            return false;
        }

        return true;
    }
}

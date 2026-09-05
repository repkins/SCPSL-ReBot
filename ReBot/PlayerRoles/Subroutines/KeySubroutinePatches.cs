using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Subroutines;
using ReBot.Main.AI;

namespace ReBot.PlayerRoles.Subroutines;

[HarmonyPatch(typeof(KeySubroutine<Scp939Role>))]
internal static class KeySubroutineScp939Patches
{
    [HarmonyPatch("KeyPressable", MethodType.Getter)]
    [HarmonyPrefix()]
    public static bool SetResultTrueIfBot(KeySubroutine<Scp939Role> __instance, ref bool __result)
    {
        return KeySubroutinePatches.SetResultTrueIfBot(__instance, ref __result);
    }
}

[HarmonyPatch(typeof(KeySubroutine<Scp096Role>))]
internal static class KeySubroutineScp096Patches
{
    [HarmonyPatch("KeyPressable", MethodType.Getter)]
    [HarmonyPrefix()]
    public static bool SetResultTrueIfBot(KeySubroutine<Scp096Role> __instance, ref bool __result)
    {
        return KeySubroutinePatches.SetResultTrueIfBot(__instance, ref __result);
    }
}

internal static class KeySubroutinePatches
{
    public static bool SetResultTrueIfBot(SubroutineBase subroutineBase, ref bool result)
    {
        if (subroutineBase.Role.TryGetOwner(out var hub)
            && BotStore.BotHubRefs.ContainsKey(hub))
        {
            result = true;

            return false;
        }

        return true;
    }
}

using HarmonyLib;
using PlayerRoles;
using ReBot.Main.AI;

namespace ReBot.PlayerRoles;

[HarmonyPatch(typeof(PlayerRoleBase))]
internal static class PlayerRoleBasePatches
{
    [HarmonyPatch(nameof(PlayerRoleBase.IsControllable), MethodType.Getter)]
    [HarmonyPostfix]
    public static void ReturnTrueIfBot(PlayerRoleBase __instance, ref bool __result)
    {
        if (!__result && __instance.TryGetOwner(out var hub) && BotStore.BotHubRefs.ContainsKey(hub))
        {
            __result = true;
        }
    }
}

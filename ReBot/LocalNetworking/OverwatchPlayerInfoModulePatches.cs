using HarmonyLib;
using Mirror;
using PlayerRoles.Spectating;
using ReBot.Main.AI;

namespace ReBot.LocalNetworking;

[HarmonyPatch(typeof(OverwatchPlayerInfoModule))]
internal static class OverwatchPlayerInfoModulePatches
{
    [HarmonyPatch(nameof(OverwatchPlayerInfoModule.ServerWriteRpc))]
    [HarmonyPrefix]
    public static bool SendZeroIfSpectatedIsBot(OverwatchPlayerInfoModule __instance, NetworkWriter writer)
    {
        if (__instance.Role is not OverwatchRole overwatchRole 
            || !ReferenceHub.TryGetHubNetID(overwatchRole.SyncedSpectatedNetId, out var referenceHub)
            || BotStore.BotHubRefs.ContainsKey(referenceHub))
        {
            writer.WriteInt(0);
            return false;
        }

        return true;
    }
}

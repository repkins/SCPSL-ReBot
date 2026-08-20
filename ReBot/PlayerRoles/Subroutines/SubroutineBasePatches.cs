using HarmonyLib;
using PlayerRoles.Subroutines;
using ReBot.Main.AI;

namespace ReBot.PlayerRoles.Subroutines;

[HarmonyPatch(typeof(SubroutineBase))]
internal static class SubroutineBasePatches
{
    [HarmonyPatch("ClientSendCmd")]
    [HarmonyPrefix]
    public static bool SendCmdIfBot(SubroutineBase __instance)
    {
        if (__instance.Role.TryGetOwner(out var hub) && BotStore.BotHubRefs.TryGetValue(hub, out var botRef))
        {
            var connectionToServer = BotStore.Bots[botRef.Idx].ConnectionToServer;
            connectionToServer.Send<SubroutineMessage>(new(__instance, false));

            return false;
        }

        return true;
    }
}

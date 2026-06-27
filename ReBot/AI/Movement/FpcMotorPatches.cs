using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using System.Reflection;
using UnityEngine;

namespace ReBot.AI.Movement
{
    [HarmonyPatch(typeof(FpcMotor))]
    internal static class FpcMotorPatches
    {
        [HarmonyPatch("DesiredMove", MethodType.Getter)]
        [HarmonyPrefix()]
        public static bool GetBotDesiredMoveIfBot(FpcMotor __instance, ref Vector3 __result)
        {
            var fpcModule = MainModuleField.GetValue(__instance) as FirstPersonMovementModule;
            var hub = HubGetter.Invoke(fpcModule, null) as ReferenceHub;

            if (BotStore.BotHubRefs.TryGetValue(hub, out var botRef))
            {
                __result = fpcModule!.transform.TransformDirection(MovementStore.DesiredMoves[botRef.Idx].DesiredLocalDirection);
                MovementStore.DesiredMoves[botRef.Idx] = new(Vector3.zero);


                return false;
            }

            return true;
        }

        [HarmonyPatch("GetFrameMove")]
        [HarmonyPrefix()]
        public static bool AssignNewReceivedPositionIfBot(FpcMotor __instance, ref Vector3 __result)
        {
            var fpcModule = MainModuleField.GetValue(__instance) as FirstPersonMovementModule;
            var hub = HubGetter.Invoke(fpcModule, null) as ReferenceHub;

            if (BotStore.BotHubRefs.TryGetValue(hub, out var botRef))
            { 
                __instance.ReceivedPosition = new RelativePositioning.RelativePosition(fpcModule!.Position + __instance.MoveDirection);
            }

            return true;
        }

        private static readonly FieldInfo MainModuleField = AccessTools.Field(typeof(FpcMotor), "MainModule");
        private static readonly MethodInfo HubGetter = AccessTools.PropertyGetter(typeof(FirstPersonMovementModule), "Hub");
    }
}

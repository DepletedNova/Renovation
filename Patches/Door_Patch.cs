using HarmonyLib;
using Kitchen;

namespace KitchenRenovation.Patches
{
    [HarmonyPatch(typeof(Door))]
    class Door_Patch
    {
        [HarmonyPatch(nameof(Door.Update))]
        [HarmonyPrefix]
        private static bool Update_Prefix(bool is_door, Door __instance)
        {
            return __instance.HatchGameObject.activeSelf || !(!__instance.IsCurrentlyDisabled && is_door);
        }
    }
}

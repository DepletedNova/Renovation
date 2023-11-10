using HarmonyLib;
using Kitchen;

namespace KitchenRenovation.Patches
{
    [HarmonyPatch(typeof(LayoutView))]
    class LayoutView_Patch
    {
        [HarmonyPatch(nameof(LayoutView.Initialise))]
        [HarmonyPostfix]
        private static void Initialise_Postfix(LayoutView __instance)
        {
            Views.RenovationView.Layout = __instance;
        }
    }
}

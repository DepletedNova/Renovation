using HarmonyLib;
using Kitchen;
using Unity.Entities;

namespace KitchenRenovation.Patches
{
    [HarmonyPatch(typeof(DeskPickTarget))]
    static class DeskPickTarget_Patch
    {
        [HarmonyPatch("MeetsConditions")]
        [HarmonyPostfix]
        static void MeetsConditions_Postfix(Entity ent, CDeskTarget conditions, bool needs_enchant, DeskPickTarget __instance, ref bool __result)
        {
            if (!conditions.RequireCopyable || !__result)
                return;

            var EM = __instance.EntityManager;
            if (!EM.HasComponent<CBlueprintStore>(ent))
                return;

            var cBlueprint = EM.GetComponentData<CBlueprintStore>(ent);

            if (BlocksDesk(cBlueprint.ApplianceID))
                __result = false;
        }
    }
}

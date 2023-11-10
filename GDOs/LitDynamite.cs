using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class LitDynamite : CustomAppliance
    {
        public override string UniqueNameID => "Lit Dynamite";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Lit Dynamite", "", new List<Appliance.Section>(), new()))
        };

        public override EntryAnimation EntryAnimation => EntryAnimation.Instant;

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CDoesNotOccupy(),
            new CTakesDuration
            {
                Total = 5f
            },
            new CDisplayDuration
            {
                Process = GetCustomGameDataObject<BombProcess>().ID
            },
            new CExplodeAfterDuration()
            {
                Width = 3,
                Length = 3,
                DestroyAppliances = false,
            }
        };

        public override GameObject Prefab => GetPrefab("Lit Dynamite");
        public override void SetupPrefab(GameObject prefab)
        {
            Dynamite.SetupMaterials(prefab);
            var fuse = prefab.GetChild("Fuse");
            fuse.ApplyMaterial<ParticleSystemRenderer>(MaterialUtils.GetExistingMaterial("Plastic - Yellow"));
        }
    }
}

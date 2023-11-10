using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using KitchenRenovation.Views;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace KitchenRenovation.GDOs
{
    public class MobileDynamite : CustomAppliance
    {
        public override string UniqueNameID => "Mobile Dynamite";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Mobile Dynamite", "", new List<Appliance.Section>(), new()))
        };

        public override EntryAnimation EntryAnimation => EntryAnimation.Instant;

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CDoesNotOccupy(),
            new CTakesDuration
            {
                Total = 7f
            },
            new CDisplayDuration
            {
                Process = GetCustomGameDataObject<BombProcess>().ID
            },
            new CExplodeAfterDuration()
            {
                Width = 3,
                Length = 3,
                DestroyAppliances = true,
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

using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using KitchenRenovation.Views;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class CreativeDynamite : CustomAppliance
    {
        public override string UniqueNameID => "Lit Dynamite";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Creative Dynamite", "For creative usage!", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Fuse",
                    Description = "Interact to light the fuse",
                    RangeDescription = "Only at night"
                },
                new()
                {
                    Title = "Explosive",
                    Description = "Blows up and destroys nearby walls",
                    RangeDescription = "<sprite name=\"range\"> 3x3 Tiles"
                }
            }, new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CFixedRotation(),
            new CTakesDuration
            {
                Total = 5f,
                Mode = InteractionMode.Appliances
            },
            new CDisplayDuration
            {
                Process = GetCustomGameDataObject<BombProcess>().ID
            },
            new CDynamite
            {
                UseAtNight = true
            },
            new CIsInactive(),
            new CExplodeAfterDuration()
            {
                Width = 3,
                Length = 3,
                DestroyAppliances = false,
            },
        };

        public override GameObject Prefab => GetPrefab("Creative Dynamite");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Charge", "Plastic - Blue", "Wood - Corkboard", "Clothing Black");
            prefab.TryAddComponent<LitFuseView>().Fuse =
                prefab.GetChild("Fuse").ApplyMaterial<ParticleSystemRenderer>(MaterialUtils.GetExistingMaterial("Plastic - Yellow")).GetComponent<ParticleSystem>();
        }
    }
}

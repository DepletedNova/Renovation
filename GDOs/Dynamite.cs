using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using KitchenRenovation.Views;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class Dynamite : CustomAppliance, IRequirePreference, IBlockDesks
    {
        public string PreferenceName() => "Dynamite";

        public override string UniqueNameID => "Dynamite";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Dynamite", "A shaped charge in spirit", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Limited",
                    Description = "Cannot be copied. One time use"
                },
                new()
                {
                    Title = "Explosive",
                    Description = "Blows up and destroys nearby walls",
                    RangeDescription = "<sprite name=\"range\"> 3x3 Tiles"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override int PurchaseCostOverride => 500;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => RemovalShoppingTag;
        public override OccupancyLayer Layer => OccupancyLayer.Wall;

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CTakesDuration
            {
                Total = 5f
            },
            new CDisplayDuration
            {
                Process = GetCustomGameDataObject<BombProcess>().ID
            },
            new CDynamite(),
            new CIsInactive(),
            new CBreachAfterDuration
            {
                DestroyDistance = 1,
                HatchDistance = 1
            }
        };

        public override GameObject Prefab => GetPrefab("Dynamite");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Charge", "Plastic - Red", "Wood - Corkboard", "Clothing Black");
            prefab.TryAddComponent<LitFuseView>().Fuse =
                prefab.GetChild("Fuse").ApplyMaterial<ParticleSystemRenderer>(MaterialUtils.GetExistingMaterial("Plastic - Yellow")).GetComponent<ParticleSystem>();
        }

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, SledgehammerSource>(),
        };
    }
}

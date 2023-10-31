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
    public class WallDrill : CustomAppliance
    {
        public override string UniqueNameID => "Wall Drill";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Drill", "Drills through appliances and walls!", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Driller",
                    Description = "Drives forward during the day, destroying appliances and walls in its path",
                    RangeDescription = "<sprite name=\"range\"> 5 Tiles"
                },
                new()
                {
                    Title = "Fueled",
                    Description = "Requires payment per day of use except for the day of purchase",
                    RangeDescription = "<sprite name=\"coin\"> 120"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override int PurchaseCostOverride => 500;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Misc | ShoppingTags.Technology;

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CTakesDuration
            {
                Total = 2f,
                Manual = true,
                Mode = InteractionMode.Appliances,
                ManualNeedsEmptyHands = true,
            },
            new CSpawnBoughtAppliance
            {
                MobileAppliance = GetCustomGameDataObject<MobileWallDrill>().ID
            },
            new CCanBeDailyPurchased
            {
                Count = 120
            },
            new CHasDailyPurchase(),
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Base", "Metal Dark");
            prefab.ApplyMaterialToChildren("Light", "Indicator Light");
            
            prefab.TryAddComponent<NightObjectView>().Object = SetupDrillMaterials(prefab.GetChild("Drill"));
        }

        internal static GameObject SetupDrillMaterials(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Chassis", "Metal - Brass");
            prefab.ApplyMaterialToChild("Engine", "Metal Dark", "Hob Black");
            prefab.ApplyMaterialToChild("Wheels", "Metal Dark", "Metal");
            prefab.ApplyMaterialToChild("Treads", "Metal Black");
            prefab.ApplyMaterialToChild("Drill", "Metal");
            return prefab;
        }
    }
}

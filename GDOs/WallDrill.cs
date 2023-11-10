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
    public class WallDrill : CustomAppliance, IRequirePreference, IBlockDesks
    {
        public string PreferenceName() => "WallDrill";

        public override string UniqueNameID => "Wall Drill";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Drill", "Quite destructive!", new List<Appliance.Section>()
            {
                new()
                {
                    Description = "Can not be copied or discounted"
                },
                new()
                {
                    Title = "Emergency Switch",
                    Description = "Can be interacted to be shut off during the day"
                },
                new()
                {
                    Title = "Unstoppable",
                    Description = "Drives forward during the day and destroys walls. Can be blocked by appliances.",
                    RangeDescription = "<sprite name=\"range\"> 6 Tiles"
                },
                new()
                {
                    Title = "Fueled",
                    Description = "Requires payment per day of use. Payment increases in overtime.",
                    RangeDescription = "<sprite name=\"coin\"> 500"
                },
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override RarityTier RarityTier => RarityTier.Common;
        public override ShoppingTags ShoppingTags => RenovationDestructiveTag;

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
            new CDisplayDuration
            {
                Process = ProcessReferences.Purchase
            },
            new CSpawnSpecialMobile
            {
                ID = GetCustomGameDataObject<MobileWallDrill>().ID
            },
            new CPurchaseable
            {
                Cost = 250
            },
            new CRampingCost
            {
                IncreasedCost = 25,
                DayIncrement = 1,
                MinimumDay = 15,
                UseBoughtDay = false
            },
            new CIsDailyPurchase()
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Base", "Metal Very Dark");
            
            prefab.TryAddComponent<NightObjectView>().Object = SetupMaterials(prefab.GetChild("Drill"));
            prefab.TryAddComponent<PurchaseLightView>().Renderer = prefab.ApplyMaterialToChild("Light", "Indicator Light On").GetComponent<MeshRenderer>();
        }

        internal static GameObject SetupMaterials(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Chassis", "Metal - Brass");
            prefab.ApplyMaterialToChild("Engine", "Metal Dark", "Hob Black");
            prefab.ApplyMaterialToChild("Wheels", "Metal Very Dark", "Metal Dark");
            prefab.ApplyMaterialToChild("Treads", "Metal Black");
            prefab.ApplyMaterialToChild("Drill", "Metal Dark");
            return prefab;
        }
    }
}

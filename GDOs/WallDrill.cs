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
    public class WallDrill : CustomAppliance
    {
        public override string UniqueNameID => "Wall Drill";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Drill", "Quite destructive!", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Emergency Switch",
                    Description = "Can be interacted to be shut off during the day"
                },
                new()
                {
                    Title = "Driller",
                    Description = "Drives forward during the day and destroys both appliances and walls",
                    RangeDescription = "<sprite name=\"range\"> 6 Tiles"
                },
                new()
                {
                    Title = "Fueled",
                    Description = "Requires payment per day of use. Increases by 10 for every day after day 5.",
                    RangeDescription = "<sprite name=\"coin\"> 350"
                },
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.ExtremelyExpensive;
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
                IncreasedCost = 10,
                DayIncrement = 1,
                MinimumDay = 5
            },
            new CIsDailyPurchase()
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Base", "Metal Very Dark");
            
            prefab.TryAddComponent<NightObjectView>().Object = SetupDrillMaterials(prefab.GetChild("Drill"));
            prefab.TryAddComponent<PurchaseLightView>().Renderer = prefab.ApplyMaterialToChild("Light", "Indicator Light On").GetComponent<MeshRenderer>();
        }

        internal static GameObject SetupDrillMaterials(GameObject prefab)
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

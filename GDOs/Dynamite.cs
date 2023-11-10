using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class Dynamite : CustomAppliance, IHavePreference
    {
        public string PreferenceName() => "Dynamite";

        public override string UniqueNameID => "Dynamite";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Dynamite", "Not quite a shaped charge", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Explosive",
                    Description = "Blows up and destroys nearby walls and appliances.",
                    RangeDescription = "<sprite name=\"range\"> 3x3 Tiles"
                },
                new()
                {
                    Title = "Fueled",
                    Description = "Requires payment to use. Increases by 25 multiplied by the current day.",
                    RangeDescription = "<sprite name=\"coin\"> 250 + x"
                },
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
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
                ID = GetCustomGameDataObject<MobileDynamite>().ID
            },
            new CPurchaseable
            {
                Cost = 250
            },
            new CRampingCost
            {
                IncreasedCost = 25,
                DayIncrement = 1
            },
            new CDestroyAfterSpawning()
        };

        public override GameObject Prefab => GetPrefab("Dynamite");
        public override void SetupPrefab(GameObject prefab) => SetupMaterials(prefab);

        internal static GameObject SetupMaterials(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Charge", "Plastic - Red", "Wood - Corkboard", "Clothing Black");
            return prefab;
        }
    }
}

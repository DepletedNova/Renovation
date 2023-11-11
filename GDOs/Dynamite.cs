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
    public class Dynamite : CustomAppliance, IRequirePreference, IBlockDesks
    {
        public string PreferenceName() => "Dynamite";

        public override string UniqueNameID => "Dynamite";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Dynamite", "Not quite a shaped charge", new List<Appliance.Section>()
            {
                new()
                {
                    Description = "Can not be copied or discounted"
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
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override int PurchaseCostOverride => 750;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => RenovationDestructiveTag;

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CSpawnSpecialMobile
            {
                ID = GetCustomGameDataObject<LitDynamite>().ID
            },
            new CDestroyAfterSpawning(),
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

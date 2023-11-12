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
            (Locale.English, CreateApplianceInfo("Drill", "Drives forward during the day and destroys any walls or hatches. Blocked by appliances", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Limited",
                    Description = "Cannot be copied. One time use"
                },
                new()
                {
                    Title = "Emergency Switch",
                    Description = "Interact to remove"
                },
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override int PurchaseCostOverride => 750;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => RemovalShoppingTag;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, Dynamite>(),
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CSpawnSpecialMobile
            {
                ID = GetCustomGameDataObject<MobileWallDrill>().ID,
            },
            new CDestroyAfterSpawning(),
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.TryAddComponent<NightObjectView>().Object = SetupMaterials(prefab.GetChild("Drill"));
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

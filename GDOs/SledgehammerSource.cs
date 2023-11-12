using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class SledgehammerSource : CustomAppliance, IBlockDesks
    {
        public override string UniqueNameID => "Sledgehammer Source";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Sledgehammer", "Destroys walls to a hatch", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Limited",
                    Description = "Cannot be copied. One time use"
                },
                new()
                {
                    Description = "Can only be used once"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override RarityTier RarityTier => RarityTier.Uncommon;
        public override ShoppingTags ShoppingTags => MiscShoppingTag | RemovalShoppingTag;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, WallDrill>(),
        };

        public override List<IApplianceProperty> Properties => new()
        {
            GetCItemProvider(GetCustomGameDataObject<Sledgehammer>().ID, 1, 1, false, false, true, true, false, false, false)
        };

        public override GameObject Prefab => GetPrefab("Sledgehammer Source");
        public override void SetupPrefab(GameObject prefab) => Sledgehammer.SetupMaterials(prefab);
    }
}

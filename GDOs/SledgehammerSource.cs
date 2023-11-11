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
            (Locale.English, CreateApplianceInfo("Sledgehammer", "Provides a sledgehammer", new List<Appliance.Section>()
            {
                new()
                {
                    Description = "Can not be copied or discounted"
                },
                new()
                {
                    Title = "Destructive",
                    Description = "Interacting with a wall during the day will destroy it to a hatch",
                },
                new()
                {
                    Title = "Limited",
                    Description = "Sledgehammers can only be used twice"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => RenovationUtilityTag;

        public override List<IApplianceProperty> Properties => new()
        {
            GetCItemProvider(GetCustomGameDataObject<Sledgehammer>().ID, 1, 1, false, false, true, true, false, false, false)
        };

        public override GameObject Prefab => GetPrefab("Sledgehammer Source");
        public override void SetupPrefab(GameObject prefab) => Sledgehammer.SetupMaterials(prefab);
    }
}

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
    public class Doorstop : CustomAppliance, IHavePreference
    {
        public string PreferenceName() => "Doorstop";

        public override string UniqueNameID => "Doorstopper";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Doorstop", "Keeps adjacent doors open", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Stopping",
                    Description = "During the day, nearby doors will be opened in the direction of the doorstop."
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Medium;
        public override RarityTier RarityTier => RarityTier.Uncommon;
        public override ShoppingTags ShoppingTags => ShoppingTags.Misc;
        public override OccupancyLayer Layer => OccupancyLayer.Floor;

        public override List<IApplianceProperty> Properties => new()
        {
            new CFixedRotation(),
            new CDoorstop(),
            new CFireImmune(),
            new CNightObject(),
        };

        public override GameObject Prefab => GetPrefab("Doorstop");

        public override void SetupPrefab(GameObject prefab)
        {
            prefab.TryAddComponent<NightObjectView>().Object = prefab.ApplyMaterialToChild("Stop", "Metal Black");
        }
    }
}

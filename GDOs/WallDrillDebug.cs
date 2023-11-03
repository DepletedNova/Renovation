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
    public class WallDrillDebug : CustomAppliance
    {
        public override string UniqueNameID => "Wall Drill Debug";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Debug Drill", "I didn't want to pay", new List<Appliance.Section>()
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
                    RangeDescription = "<sprite name=\"range\"> 4 Tiles"
                },
                new()
                {
                    Title = "Not Fueled",
                    Description = "Doesn't require payment per day of use",
                    RangeDescription = "<sprite name=\"coin\"> :)"
                },
            }, new()))
        };

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
                Cost = 0
            },
            new CHasDailyPurchase()
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Base", "Metal Very Dark");
            
            prefab.TryAddComponent<NightObjectView>().Object = SetupDrillMaterials(prefab.GetChild("Drill"));
            prefab.TryAddComponent<PurchaseLightView>().Renderer = 
                prefab.ApplyMaterialToChild("Light", "Indicator Light").GetComponent<MeshRenderer>();
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

using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenRenovation.Components;
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
                    RangeDescription = "<sprite name=\"range\"> 6 Tiles"
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
            new CDisplayDuration
            {
                Process = ProcessReferences.Purchase
            },
            new CSpawnSpecialMobile
            {
                ID = GetCustomGameDataObject<MobileWallDrill>().ID
            },
            new CNightObject()
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
    }
}

using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenRenovation.Components;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class WallDrillDebug : CustomAppliance, IWontRegister
    {
        public override string UniqueNameID => "Wall Drill Debug";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Debug Drill", "I didn't want to pay nor am I updating the sections", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Driller",
                    Description = "Drives forward during the day and destroys walls"
                },
            }, new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CSpawnSpecialMobile
            {
                ID = GetCustomGameDataObject<MobileWallDrill>().ID
            },
            new CNightObject()
        };

        public override GameObject Prefab => GetPrefab("Wall Drill");
    }
}

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
    public class DynamiteDebug : CustomAppliance
    {
        public override string UniqueNameID => "Dynamite Debug";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Debug Dynamite", "Not quite a shaped charge", new List<Appliance.Section>()
            {
                new()
                {
                    Title = "Explosive",
                    Description = "Blows up and destroys nearby walls and appliances.",
                    RangeDescription = "<sprite name=\"range\"> 3x3 Tiles"
                }
            }, new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CSpawnSpecialMobile
            {
                ID = GetCustomGameDataObject<MobileDynamite>().ID
            },
            new CDestroyAfterSpawning()
        };

        public override GameObject Prefab => GetPrefab("Dynamite");
    }
}

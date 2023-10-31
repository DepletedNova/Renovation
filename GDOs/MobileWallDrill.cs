using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenRenovation.Components;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class MobileWallDrill : CustomAppliance
    {
        public override string UniqueNameID => "Mobile Wall Drill";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Mobile Wall Drill", "", new List<Appliance.Section>(), new()))
        };

        public override EntryAnimation EntryAnimation => EntryAnimation.Instant;

        public override List<IApplianceProperty> Properties => new()
        {
            new CDoesNotOccupy(),
            new CFireImmune(),
            new CTakesDuration
            {
                Total = 5f,
                Mode = InteractionMode.Items,
                IsInverse = true,
                IsLocked = true,
            },
            new CDisplayDuration
            {
                Process = ProcessReferences.Chop
            },
            new CDestructiveAppliance
            {
                TileRange = 5f,
                Speed = 0.25f,
                DestroysAppliances = true,
                DestroyApplianceTime = 5f,
                DestroysWalls = true,
                DestroyWallTime = 5f,
            },
        };

        public override GameObject Prefab => GetPrefab("Mobile Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            WallDrill.SetupDrillMaterials(prefab);
        }
    }
}

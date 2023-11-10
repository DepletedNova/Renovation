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
    public class MobileWallDrill : CustomAppliance
    {
        public override string UniqueNameID => "Mobile Wall Drill";
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Mobile Drill", "", new List<Appliance.Section>(), new()))
        };

        public override EntryAnimation EntryAnimation => EntryAnimation.Instant;

        public override List<IApplianceProperty> Properties => new()
        {
            new CAllowMobilePathing(),
            new CDoesNotOccupy(),
            new CTakesDuration
            {
                Total = 5,
                Mode = InteractionMode.Items,
                IsLocked = true,
            },
            new CDisplayDuration
            {
                Process = GetCustomGameDataObject<DestroyWallProcess>().ID,
                IsBad = true,
            },
            new CDestructive
            {
                Multiplier = 1f,
                TargetAppliances = false,
                DestroyToWall = true,
                TargetPosition = Vector3.right * 100,
                ApplianceOffset = 0.2f,
                WallOffset = 0f
            },
            new CForwardMobile
            {
                IgnoreAppliances = false,
                IgnoreWalls = true,
                Speed = 0.35f
            },
            new CInteractDisable()
        };

        public override GameObject Prefab => GetPrefab("Mobile Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            WallDrill.SetupMaterials(prefab);
            var view = prefab.TryAddComponent<DestructiveDrillView>();
            view.Drill = prefab.transform.Find("Drill");
            view.Engine = prefab.transform.Find("Engine");
            view.Particles = prefab.GetChild("Particle").GetComponent<ParticleSystem>();

            prefab.GetChild("Particle").ApplyMaterial<ParticleSystemRenderer>(MaterialUtils.GetExistingMaterial("Wood - Corkboard"));
        }
    }
}

using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using KitchenRenovation.Views;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
                TargetAppliances = true,
                DestroyToWall = true,
                TargetPosition = Vector3.right * 100,
                ApplianceOffset = 0.2f,
                WallOffset = 0f
            },
            new CForwardMobile
            {
                IgnoreAppliances = true,
                IgnoreWalls = true,
                MaxDistance = 4,
                Speed = 0.3f
            },
            new CInteractDisable()
        };

        public override GameObject Prefab => GetPrefab("Mobile Wall Drill");
        public override void SetupPrefab(GameObject prefab)
        {
            WallDrill.SetupDrillMaterials(prefab);
            /*var view = prefab.TryAddComponent<DrillView>();
            view.Drill = prefab.transform.Find("Drill");
            view.Particles = prefab.GetChild("Particle").GetComponent<ParticleSystem>();

            view.Exhausts = new();
            for (int i = 0; i < prefab.GetChildCount(); i++)
                if (prefab.GetChild(i).name.ToLower().Contains("exhaust"))
                    view.Exhausts.Add(prefab.GetChild(i).TryAddComponent<VisualEffect>());

            foreach (var effect in Resources.FindObjectsOfTypeAll<VisualEffectAsset>())
            {
                if (effect.name == "Burning" && !view.Exhausts.IsNullOrEmpty())
                {
                    foreach (var exhaust in view.Exhausts)
                    {
                        exhaust.visualEffectAsset = effect;
                    }
                    break;
                }
            }*/
        }
    }
}

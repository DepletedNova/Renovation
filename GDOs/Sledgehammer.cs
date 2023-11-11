using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenRenovation.GDOs
{
    public class Sledgehammer : CustomItem
    {
        public override string UniqueNameID => "Sledgehammer";
        public override ToolAttachPoint HoldPose => ToolAttachPoint.Hand;
        public override ItemCategory ItemCategory => ItemCategory.ProviderOnly;
        public override ItemStorage ItemStorageFlags => ItemStorage.None;

        public override List<IItemProperty> Properties => new()
        {
            new CEquippableTool { CanHoldItems = false },
            new CDestructive
            {
                Multiplier = 1f,
            },
            new CDestructiveTool
            {
                MaxUses = 2
            }
        };

        public override GameObject Prefab => GetPrefab("Sledgehammer");
        public override void SetupPrefab(GameObject prefab) => SetupMaterials(prefab);

        internal static GameObject SetupMaterials(GameObject prefab) =>
            prefab.ApplyMaterialToChild("Sledge", "Wood 1", "Paper - Black", "Metal Very Dark");
    }
}

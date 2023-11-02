using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class PurchaseLightView : UpdatableObjectView<PurchaseLightView.ViewData>
    {
        public MeshRenderer Renderer;

        [SerializeField] public Color ActiveColor = Color.green;
        [SerializeField] public Color DisabledColor = Color.red;

        protected override void UpdateData(ViewData data)
        {
            if (Renderer != null)
                Renderer.material.color = data.Enabled ? ActiveColor : DisabledColor;
        }

        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public bool Enabled;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<PurchaseLightView>();

            public bool IsChangedFrom(ViewData check) => Enabled != check.Enabled;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Purchaseables;
            protected override void Initialise()
            {
                base.Initialise();
                Purchaseables = GetEntityQuery(new QueryHelper()
                    .Any(typeof(CCanBeDailyPurchased))
                    .All(typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                using (var views = Purchaseables.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    using var entities = Purchaseables.ToEntityArray(Allocator.Temp);
                    for (int i = 0; i < views.Length; i++)
                    {
                        SendUpdate(views[i], new ViewData
                        {
                            Enabled = Has<CHasDailyPurchase>(entities[i])
                        }, MessageType.SpecificViewUpdate);
                    }
                }
            }
        }
    }
}

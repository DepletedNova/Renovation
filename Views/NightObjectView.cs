using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class NightObjectView : UpdatableObjectView<NightObjectView.ViewData>
    {
        public GameObject Object;

        protected override void UpdateData(ViewData data)
        {
            Object.SetActive(data.ShowAppliance);
        }

        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public bool ShowAppliance;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<NightObjectView>();

            public bool IsChangedFrom(ViewData check) => ShowAppliance != check.ShowAppliance;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Query;
            protected override void Initialise()
            {
                base.Initialise();
                Query = GetEntityQuery(new QueryHelper()
                    .Any(typeof(CPurchaseable), typeof(CNightObject))
                    .All(typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                using var entities = Query.ToEntityArray(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    SendUpdate(GetComponent<CLinkedView>(entity), new ViewData
                    {
                        ShowAppliance = HasSingleton<SIsNightTime>() || (Has<CPurchaseable>(entity) && !Has<CHasPurchase>(entity))
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }
    }
}
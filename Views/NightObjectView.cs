using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace KitchenRenovation.Views
{
    public class NightObjectView : UpdatableObjectView<NightObjectView.ViewData>
    {
        public GameObject Object;

        protected override void UpdateData(ViewData data)
        {
            // for some reason SetActive does not like ternary conditionals
            if (data.ShowAppliance)
                Object.SetActive(true);
            else Object.SetActive(false);
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
                    .Any(typeof(CSpawnMobileAppliance), typeof(CSpawnBoughtAppliance), typeof(CNightObject))
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
                        ShowAppliance = HasSingleton<SIsNightTime>() || (Has<CSpawnBoughtAppliance>(entity) && !Has<CHasDailyPurchase>(entity))
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }
    }
}

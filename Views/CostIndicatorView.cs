using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using TMPro;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Views
{
    public class CostIndicatorView : UpdatableObjectView<CostIndicatorView.ViewData>
    {
        public TextMeshPro Text;

        protected override void UpdateData(ViewData data)
        {
            if (Text != null)
                Text.text = string.Format("{0}{1} <sprite name=\"coin\" color=#FF9800>", data.IsAffordable ? string.Empty : "<color=#ff1111>", data.Cost.ToString());
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public int Cost;
            [Key(2)] public bool IsAffordable;

            public bool IsChangedFrom(ViewData check) => Cost != check.Cost || IsAffordable != check.IsAffordable;  
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Indicators;
            protected override void Initialise()
            {
                base.Initialise();
                Indicators = GetEntityQuery(typeof(CLinkedView), typeof(CCostIndicator));
                RequireForUpdate(Indicators);
                RequireSingletonForUpdate<SMoney>();
            }

            protected override void OnUpdate()
            {
                var views = Indicators.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                var costs = Indicators.ToComponentDataArray<CCostIndicator>(Allocator.Temp);
                var money = GetSingleton<SMoney>().Amount;
                for (int i = 0; i < views.Length; i++)
                {
                    var cost = costs[i].Cost;
                    SendUpdate(views[i], new ViewData
                    {
                        Cost = cost,
                        IsAffordable = money > cost
                    });
                }
            }
        }
    }
}

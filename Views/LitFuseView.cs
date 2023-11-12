using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class LitFuseView : UpdatableObjectView<LitFuseView.ViewData>
    {
        public ParticleSystem Fuse;

        protected override void UpdateData(ViewData data)
        {
            if (data.Lit)
                Fuse.Play();
            else
                Fuse.Stop();
        }

        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool Lit;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<LitFuseView>();

            public bool IsChangedFrom(ViewData check) => Lit != check.Lit;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Query;
            protected override void Initialise()
            {
                base.Initialise();
                Query = GetEntityQuery(typeof(CLinkedView), typeof(CDynamite));
            }

            protected override void OnUpdate()
            {
                using var entities = Query.ToEntityArray(Allocator.Temp);
                using var views = Query.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    SendUpdate(views[i], new()
                    {
                        Lit = !Has<CIsInactive>(entities[i])
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }
    }
}

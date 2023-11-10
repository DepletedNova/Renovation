using Kitchen;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using MessagePack;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class ParticleView : UpdatableObjectView<ParticleView.ViewData>
    {
        private Dictionary<ParticleEvent, GameObject> Particles = new();

        public override void Initialise()
        {
            base.Initialise();
            Particles = new()
            {
                { ParticleEvent.Explosion, gameObject.GetChild("Explosion").ApplyMaterial<ParticleSystemRenderer>(MaterialUtils.GetExistingMaterial("Paper - Black")) }
            };
        }

        private GameObject ActiveObject = null;
        protected override void UpdateData(ViewData data)
        {
            if (ActiveObject != null)
                ActiveObject.SetActive(false);
            if (!Particles.TryGetValue(data.Event, out ActiveObject))
            {
                LogError($"Could not find event: {data.Event}");
                return;
            }

            ActiveObject.SetActive(true);
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public ParticleEvent Event;

            public bool IsChangedFrom(ViewData check) => Event != check.Event;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            EntityQuery Query;
            protected override void Initialise()
            {
                base.Initialise();
                Query = GetEntityQuery(typeof(CLinkedView), typeof(CParticleEvent));
            }

            protected override void OnUpdate()
            {
                using var views = Query.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using var events = Query.ToComponentDataArray<CParticleEvent>(Allocator.Temp);
                for (int i = 0; i < views.Length; i++)
                {
                    SendUpdate(views[i], new()
                    {
                        Event = events[i].Event
                    }, MessageType.ViewUpdate);
                }
            }
        }

    }
}

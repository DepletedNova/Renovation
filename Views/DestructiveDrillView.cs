using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.VFX;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class DestructiveDrillView : UpdatableObjectView<DestructiveDrillView.ViewData>
    {
        public float DrillingSpeed = 600f;
        public float BumpMin = 0.05f;
        public float BumpMax = 0.065f;

        public Transform Engine;
        public Transform Drill;
        public ParticleSystem Particles;

        private bool Active;
        private bool Drilling;
        protected override void UpdateData(ViewData data)
        {
            Active = data.Active;
            Drilling = data.Drilling;

            if (Particles != null)
            {
                if (Drilling && Active)
                    Particles.Play();
                else
                    Particles.Stop();
            }
        }

        private float CurrentSpeed = 0f;
        private void Update()
        {
            if (Drilling && Active && CurrentSpeed != DrillingSpeed)
                CurrentSpeed = Mathf.Min(CurrentSpeed + 200f * Time.deltaTime, DrillingSpeed + (Drilling ? DrillingSpeed : 0f));
            else if ((!Drilling || !Active) && CurrentSpeed > 0f)
                CurrentSpeed = Mathf.Max(CurrentSpeed - 300f * Time.deltaTime, 0f);

            if (CurrentSpeed > 0f)
                Drill.Rotate(new Vector3(0, -CurrentSpeed * Time.deltaTime, 0));

            if (Active)
            {
                Engine.transform.localPosition = Vector3.up * Random.Range(BumpMin, BumpMax);
            }
        }

        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public bool Drilling;
            [Key(2)] public bool Active;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<DestructiveDrillView>();

            public bool IsChangedFrom(ViewData check) => Drilling != check.Drilling || Active != check.Active;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Drills;
            protected override void Initialise()
            {
                base.Initialise();
                Drills = GetEntityQuery(typeof(CLinkedView), typeof(CDestructive), typeof(CPosition));
            }

            protected override void OnUpdate()
            {
                using var entities = Drills.ToEntityArray(Allocator.Temp);
                using var views = Drills.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using var destructives = Drills.ToComponentDataArray<CDestructive>(Allocator.Temp);
                for (int i = 0; i < views.Length; i++)
                {
                    var cDest = destructives[i];
                    SendUpdate(views[i], new ViewData
                    {
                        Drilling = cDest.Target != Entity.Null,
                        Active = !Has<CIsInactive>(entities[i])
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

    }
}

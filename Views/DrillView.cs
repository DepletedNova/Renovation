using Kitchen;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using MessagePack;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

namespace KitchenRenovation.Views
{
    public class DrillView : UpdatableObjectView<DrillView.ViewData>
    {
        public float IdleSpeed = 400f;
        public float DrillingSpeed = 600f;

        public Transform Drill;
        public ParticleSystem Particles;
        [SerializeField] public List<VisualEffect> Exhausts;

        private bool Active;
        private bool Drilling;
        protected override void UpdateData(ViewData data)
        {
            Active = data.Active;
            Drilling = data.Drilling;

            if (Particles != null)
            {
                if (Drilling)
                    Particles.Play();
                else
                    Particles.Stop();
            }

            if (!Exhausts.IsNullOrEmpty())
            {
                var active = Active ? 0.1f : 0f;
                foreach (var exhaust in Exhausts)
                {
                    exhaust.SetFloat("Emit Rate", active);
                }
            }
        }

        private float CurrentSpeed = 0f;
        private void Update()
        {
            if (Active && CurrentSpeed != DrillingSpeed)
                CurrentSpeed = Mathf.Min(CurrentSpeed + 150f * Time.deltaTime, DrillingSpeed + (Drilling ? DrillingSpeed : 0f));
            else if (!Active && CurrentSpeed > 0f)
                CurrentSpeed = Mathf.Max(CurrentSpeed - 200f * Time.deltaTime, 0f);

            if (CurrentSpeed > 0f)
                Drill.Rotate(new Vector3(0, -CurrentSpeed * Time.deltaTime, 0));
        }

        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public bool Drilling;
            [Key(2)] public bool Active;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<DrillView>();

            public bool IsChangedFrom(ViewData check) => Drilling != check.Drilling || Active != check.Active;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Drills;
            protected override void Initialise()
            {
                base.Initialise();
                Drills = GetEntityQuery(typeof(CLinkedView), typeof(CDestructiveAppliance));
            }

            protected override void OnUpdate()
            {
                using (var views = Drills.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    using var drills = Drills.ToComponentDataArray<CDestructiveAppliance>(Allocator.Temp);
                    for (int i = 0; i < views.Length; i++)
                    {
                        var drill = drills[i];
                        SendUpdate(views[i], new ViewData
                        {
                            Drilling = drill.DestructionTarget != Entity.Null,
                            Active = !drill.CompletedTask
                        }, MessageType.SpecificViewUpdate);
                    }
                }
            }
        }
    }
}

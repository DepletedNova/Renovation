using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    [UpdateBefore(typeof(DurationLocks))]
    public class UpdateWallAfterDuration : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CWallTargetedBy), typeof(CTargetableWall), typeof(CTakesDuration));
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var durations = Query.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var cDuration = durations[i];
                if (!cDuration.Active || cDuration.Remaining > 0)
                    continue;

                var entity = entities[i];
                var buffer = GetBuffer<CWallTargetedBy>(entity);

                if (buffer.IsEmpty)
                    continue;

                // Check type of interaction
                bool shouldCreate = false;
                bool shouldDestroy = false;
                bool OnlyHatch = false;
                for (int i2 = 0; i2 < buffer.Length; i2++)
                {
                    var item = buffer[i2];
                    shouldCreate |= item.Create;
                    shouldDestroy |= item.Destroy;
                    OnlyHatch |= !item.Hatch;

                    // Clear buffer items
                    if (Require(item.Interactor, out CDestructive cDest))
                    {
                        if (Require(item.Interactor, out CDestructiveTool cTool))
                        {
                            cTool.CurrentUses++;
                            if (cTool.MaxUses <= cTool.CurrentUses)
                            {
                                EntityManager.DestroyEntity(item.Interactor);
                                continue;
                            }
                            Set(item.Interactor, cTool);
                        }

                        cDest.Target = Entity.Null;
                        cDest.TargetPosition = Vector3.right * 100;
                        Set(item.Interactor, cDest);
                    }
                }
                buffer.Clear();
                OnlyHatch = !OnlyHatch;

                if (shouldDestroy && !Has<CRemovedWall>(entity)) // Destroy
                {
                    if (!Has<CReaching>(entity))
                    {
                        Set<CReaching>(entity);
                        Set<CHatch>(entity);
                    }
                    else if (!OnlyHatch)
                    {
                        Set<CRemovedWall>(entity);
                        EntityManager.RemoveComponent<CReaching>(entity);
                        EntityManager.RemoveComponent<CHatch>(entity);
                    }

                    CSoundEvent.Create(EntityManager, DestroySoundEvent);
                    
                    if (Require(entity, out CPosition cPos))
                    {
                        CParticleEvent.Create(EntityManager, ParticleEvent.WallDestruction, cPos);
                    }
                } 
                else if (shouldCreate) // Create
                {
                    // fix this bogus
                }

                cDuration.Total = 10f;
                cDuration.Remaining = 10f;
                cDuration.IsLocked = true;
                Set(entity, cDuration);

                if (Has<CPreventUse>(entity))
                    EntityManager.RemoveComponent<CPreventUse>(entity);

                Set<SRebuildReachability>();
            }
        }
    }
}

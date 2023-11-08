using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class DestroyWallAfterDuration : GameSystemBase
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
                bool removeToHatch = false;
                bool createHatch = false;
                for (int i2 = 0; i2 < buffer.Length; i2++)
                {
                    var item = buffer[i2];
                    shouldCreate |= item.Create;
                    removeToHatch |= item.Hatch && item.Destroy;
                    createHatch |= item.Hatch && item.Create;
                }

                if (!shouldCreate && !Has<CRemovedWall>(entity)) // Destroy
                {
                    if (!Has<CReaching>(entity) && removeToHatch)
                        Set<CReaching>(entity);
                    else if (!removeToHatch)
                        Set<CRemovedWall>(entity);
                } else 
                if (shouldCreate) // Create
                {
                    if (Has<CRemovedWall>(entity))
                        EntityManager.RemoveComponent<CRemovedWall>(entity);
                    if (createHatch)
                        Set<CReaching>(entity);
                    else if (!createHatch && Has<CReaching>(entity))
                        EntityManager.RemoveComponent<CRemovedWall>(entity);
                    Set<CPlacedWall>(entity);
                }

                // Clear out and reset interactors
                for (int i2 = buffer.Length; i2 >= 0; i2--)
                {
                    var interactor = buffer[i2].Interactor;
                    if (Require(interactor, out CDestructive cDest))
                    {
                        cDest.Target = Entity.Null;
                        cDest.TargetPosition = Vector3.right * 100;
                        Set(interactor, cDest);
                    }
                }

                buffer.Clear();
                Set<SRebuildReachability>();
            }
        }
    }
}

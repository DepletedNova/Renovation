using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class DestroyApplianceAfterDuration : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CDestructive), typeof(CTakesDuration), ComponentType.Exclude<CIsInactive>(), ComponentType.Exclude<CIsOnFire>());
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var destructives = Query.ToComponentDataArray<CDestructive>(Allocator.Temp);
            using var durations = Query.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var cDest = destructives[i];
                var cDuration = durations[i];
                if (!Has<CAppliance>(cDest.Target) || !cDuration.Active || cDuration.Remaining > 0)
                    continue;

                EntityManager.DestroyEntity(cDest.Target);

                cDest.Target = Entity.Null;
                cDest.TargetPosition = Vector3.right * 100;
                Set(entities[i], cDest);
            }
        }
    }
}

using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(DurationLocks))]
    public class DestructiveApplianceLocks : GameSystemBase
    {
        private EntityQuery Appliances;
        protected override void Initialise()
        {
            base.Initialise();
            Appliances = GetEntityQuery(typeof(CPosition), typeof(CDestructiveAppliance), typeof(CTakesDuration));
            RequireForUpdate(Appliances);
        }

        protected override void OnUpdate()
        {
            using var entities = Appliances.ToEntityArray(Allocator.Temp);
            using var durations = Appliances.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            using var destructives = Appliances.ToComponentDataArray<CDestructiveAppliance>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var duration = durations[i];
                var destructive = destructives[i];
                var target = destructive.DestructionTarget;
                duration.IsLocked = target == Entity.Null || (!Has<CAppliance>(target) && !Has<CTargetableWall>(target));
                Set(entities[i], duration);
            }
        }
    }
}

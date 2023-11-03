using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class DestroyApplianceAfterDuration : GameSystemBase
    {
        private EntityQuery Mobiles;
        protected override void Initialise()
        {
            base.Initialise();
            Mobiles = GetEntityQuery(typeof(CDestructiveAppliance), typeof(CTakesDuration));
        }

        protected override void OnUpdate()
        {
            using var entities = Mobiles.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var cDuration = GetComponent<CTakesDuration>(entity);
                var cDestructive = GetComponent<CDestructiveAppliance>(entity);

                Entity target = cDestructive.DestructionTarget;
                if (!cDuration.Active || cDuration.Remaining > 0 || target == Entity.Null || !Has<CAppliance>(target))
                    continue;

                cDestructive.DestructionTarget = Entity.Null;
                Set(entity, cDestructive);

                ECBs[ECB.DestructionGroup].CreateCommandBuffer().DestroyEntity(target);

                CSoundEvent.Create(EntityManager, DestroySoundEvent);
            }
        }
    }
}

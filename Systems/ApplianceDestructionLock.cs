using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(DurationLocks))]
    public class ApplianceDestructionLock : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CPosition), typeof(CDestructive), typeof(CTakesDuration));
        }

        protected override void OnUpdate()
        {
            using (var entities = Query.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var cPosition = GetComponent<CPosition>(entity);
                    var cDestructive = GetComponent<CDestructive>(entity);
                    var cDuration = GetComponent<CTakesDuration>(entity);

                    cDuration.IsLocked =
                        Has<CIsOnFire>(entity) || Has<CIsInactive>(entity) ||
                        !Has<CAppliance>(cDestructive.Target) || (cPosition.Position - cDestructive.TargetPosition).Chebyshev() > 0.1f;
                    Set(entity, cDuration);
                }
            }
        }
    }
}

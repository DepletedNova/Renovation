using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(DurationLocks))]
    public class DailyPurchaseableLocks : GameSystemBase
    {
        private EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CCanBeDailyPurchased), typeof(CTakesDuration));
            RequireForUpdate(Query);
        }

        protected override void OnUpdate()
        {
            using (var entities = Query.ToEntityArray(Allocator.Temp))
            {
                using var durations = Query.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var duration = durations[i];
                    duration.IsLocked = Has<CHasDailyPurchase>(entity) || Has<SIsDayTime>();
                    Set(entity, duration);
                }
            }
        }
    }
}

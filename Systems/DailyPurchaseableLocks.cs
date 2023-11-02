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
            RequireSingletonForUpdate<SMoney>();
        }

        protected override void OnUpdate()
        {
            using (var entities = Query.ToEntityArray(Allocator.Temp))
            {
                using var durations = Query.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
                using var purchases = Query.ToComponentDataArray<CCanBeDailyPurchased>(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var duration = durations[i];
                    var purchase = purchases[i];
                    var money = GetSingleton<SMoney>();

                    duration.IsLocked = Has<CHasDailyPurchase>(entity) || !HasSingleton<SIsNightTime>() || money.Amount < purchase.Cost;
                    Set(entity, duration);
                }
            }
        }
    }
}

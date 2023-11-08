using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(DurationLocks))]
    public class PurchaseableLock : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CPurchaseable), typeof(CTakesDuration));
            RequireSingletonForUpdate<SDay>();
            RequireSingletonForUpdate<SMoney>();
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var durations = Query.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            using var purchaseables = Query.ToComponentDataArray<CPurchaseable>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];

                int cost = purchaseables[i].Cost;
                if (Require(entity, out CRampingCost cRamping))
                {
                    var day = GetSingleton<SDay>().Day - cRamping.MinimumDay;
                    if (day > 0)
                        cost += cRamping.IncreasedCost * (day / cRamping.DayIncrement);
                }

                var duration = durations[i];
                duration.IsLocked = Has<CHasPurchase>(entity) || GetSingleton<SMoney>().Amount < cost;
                Set(entity, duration);
            }
        }
    }
}

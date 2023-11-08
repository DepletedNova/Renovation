using Kitchen;
using KitchenRenovation.Components;
using System.Net;
using Unity.Collections;
using Unity.Entities;
using static UnityEngine.EventSystems.EventTrigger;

namespace KitchenRenovation.Systems
{
    public class AddPurchaseAfterDuration : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CPurchaseable), typeof(CTakesDuration));
            RequireSingletonForUpdate<SMoney>();
            RequireSingletonForUpdate<SDay>();
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var durations = Query.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            using var purchaseables = Query.ToComponentDataArray<CPurchaseable>(Allocator.Temp);
            var totalCost = 0;
            for (int i = 0; i < entities.Length; i++)
            {
                var duration = durations[i];
                if (!duration.Active || duration.Remaining > 0)
                    continue;

                var entity = entities[i];

                int cost = purchaseables[i].Cost;
                if (Require(entity, out CRampingCost cRamping))
                {
                    var day = GetSingleton<SDay>().Day - cRamping.MinimumDay;
                    if (day > 0)
                        cost += cRamping.IncreasedCost * (day / cRamping.DayIncrement);
                }

                totalCost += cost;
                Set<CHasPurchase>(entity);
            }

            if (totalCost <= 0)
                return;

            var money = GetSingleton<SMoney>();
            money.Amount -= totalCost;
            Set(money);
        }
    }
}

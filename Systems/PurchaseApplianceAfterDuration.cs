using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class PurchaseApplianceAfterDuration : GameSystemBase
    {
        private EntityQuery RequiresPurchase;
        protected override void Initialise()
        {
            base.Initialise();
            RequiresPurchase = GetEntityQuery(typeof(CCanBeDailyPurchased), typeof(CTakesDuration));
            RequireSingletonForUpdate<SMoney>();
        }

        protected override void OnUpdate()
        {
            using (var entities = RequiresPurchase.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var duration = GetComponent<CTakesDuration>(entity);
                    if (duration.Remaining > 0 || !duration.Active)
                        continue;

                    var cost = GetComponent<CCanBeDailyPurchased>(entity).Cost;
                    var money = GetSingleton<SMoney>();

                    if (cost <= money.Amount)
                    {
                        CSoundEvent.Create(EntityManager, KitchenData.SoundEvent.ItemDelivered);
                        Set<CHasDailyPurchase>(entity);

                        money.Amount -= cost;
                        SetSingleton(money);
                    }
                }
            }
        }
    }
}

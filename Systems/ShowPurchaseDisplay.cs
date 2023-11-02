using Kitchen;
using KitchenLib.References;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class ShowPurchaseDisplay : GameSystemBase
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

                    var money = GetSingleton<SMoney>();
                    var cost = GetComponent<CCanBeDailyPurchased>(entity);

                    if (Has<CDisplayDuration>(entity))
                    {
                        if ((!Has<CBeingLookedAt>(entity) && (duration.Remaining >= duration.Total || duration.Remaining <= 0)) || money.Amount < cost.Cost ||
                            Has<CHasDailyPurchase>(entity) || !HasSingleton<SIsNightTime>())
                            EntityManager.RemoveComponent<CDisplayDuration>(entity);
                    } else
                    {
                        if (HasSingleton<SIsNightTime>() && Has<CBeingLookedAt>(entity) && !Has<CHasDailyPurchase>(entity) && money.Amount >= cost.Cost)
                        {
                            Set(entity, new CDisplayDuration
                            {
                                Process = ProcessReferences.Purchase,
                                ShowWhenEmpty = true
                            });
                        }
                    }
                }
            }
        }
    }
}

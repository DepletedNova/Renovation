using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class RemoveDailyPurchases : StartOfNightSystem
    {
        private EntityQuery Purchases;
        protected override void Initialise()
        {
            base.Initialise();
            Purchases = GetEntityQuery(typeof(CHasDailyPurchase));
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<CHasDailyPurchase>(Purchases);
        }
    }
}

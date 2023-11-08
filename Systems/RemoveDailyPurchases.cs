using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class RemoveDailyPurchases : StartOfNightSystem
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CIsDailyPurchase), typeof(CHasPurchase));
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<CHasPurchase>(Query);
        }
    }
}

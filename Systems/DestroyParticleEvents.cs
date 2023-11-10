using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class DestroyParticleEvents : NightSystem
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CParticleEvent));
            RequireForUpdate(Query);
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(Query);
        }
    }
}

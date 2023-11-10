using Kitchen;
using KitchenData;
using Unity.Entities;

namespace KitchenRenovation.Components
{
    public struct CParticleEvent : IComponentData
    {
        public ParticleEvent Event;

        public static Entity Create(EntityManager em, ParticleEvent e, CPosition cPos = default)
        {
            Entity entity = em.CreateEntity();
            em.AddComponentData(entity, new CParticleEvent { Event = e });
            em.AddComponentData(entity, new CRequiresView { Type = ParticleEventView });
            em.AddComponentData(entity, cPos);
            return entity;
        }
    }

    public enum ParticleEvent
    {
        Explosion
    }
}

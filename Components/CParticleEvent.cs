using Kitchen;
using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Components
{
    public struct CParticleEvent : IComponentData
    {
        public ParticleEvent Event;

        public static Entity Create(EntityManager em, ParticleEvent e, CPosition pos = default)
        {
            Entity entity = em.CreateEntity();
            em.AddComponentData(entity, new CParticleEvent { Event = e });
            em.AddComponentData(entity, new CRequiresView { Type = ParticleEventView });
            em.AddComponentData(entity, pos);
            return entity;
        }
    }

    public enum ParticleEvent
    {
        Explosion,
        WallDestruction
    }
}

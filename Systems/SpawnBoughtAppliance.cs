using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class SpawnBoughtAppliance : StartOfDaySystem
    {
        private EntityQuery Spawners;

        public Allocator ALlocator { get; private set; }

        protected override void Initialise()
        {
            base.Initialise();
            Spawners = GetEntityQuery(new QueryHelper()
                .All(typeof(CSpawnBoughtAppliance), typeof(CPosition))
                .Any(typeof(CHasDailyPurchase), typeof(CForceSpawn)));
        }

        protected override void OnUpdate()
        {
            using var entities = Spawners.ToEntityArray(Allocator.Temp);
            using var spawners = Spawners.ToComponentDataArray<CSpawnBoughtAppliance>(Allocator.Temp);
            using var positions = Spawners.ToComponentDataArray<CPosition>(Allocator.Temp);
            for (int i = 0; i < spawners.Length; i++)
            {
                var entity = EntityManager.CreateEntity();
                Set(entity, new CCreateAppliance { ID = spawners[i].MobileAppliance });
                Set(entity, positions[i]);
                Set<CDestroyApplianceAtNight>(entity);
                Set(entity, (CLinkedMobileBase)entities[i]);
            }
        }
    }
}

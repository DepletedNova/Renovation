using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class SpawnSpecialMobiles : StartOfDaySystem
    {
        private EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CSpawnSpecialMobile), typeof(CPosition));
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var mobiles = Query.ToComponentDataArray<CSpawnSpecialMobile>(Allocator.Temp);
            using var positions = Query.ToComponentDataArray<CPosition>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];

                if (Has<CPurchaseable>(entity) && !Has<CHasPurchase>(entity))
                    continue;

                var cPos = positions[i];
                var cMobile = mobiles[i];

                if (cMobile.InvertRotation)
                    cPos.Rotation = OrientationHelpers.Flip(cPos.Rotation.ToOrientation()).ToRotation();

                var mobile = EntityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition), typeof(CMobileBase), typeof(CDestroyApplianceAtNight));
                Set(mobile, new CCreateAppliance { ID = cMobile.ID });
                Set(mobile, cPos);
                Set(mobile, new CMobileBase
                {
                    Home = entities[i],
                    Start = cPos.Position
                });

                if (Has<CDestroyAfterSpawning>(entity))
                    EntityManager.DestroyEntity(entity);
            }
        }
    }
}

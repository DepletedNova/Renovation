using Kitchen;
using Kitchen.Layouts;
using KitchenData;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class ExplodeAfterDuration : GameSystemBase
    {
        EntityQuery Bombs;
        protected override void Initialise()
        {
            base.Initialise();
            Bombs = GetEntityQuery(typeof(CExplodeAfterDuration), typeof(CPosition), typeof(CTakesDuration));
        }
        protected override void OnUpdate()
        {
            using var entities = Bombs.ToEntityArray(Allocator.Temp);
            using var durations = Bombs.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var cDuration = durations[i];
                if (!cDuration.Active || cDuration.Remaining > 0)
                    continue;

                var entity = entities[i];
                var cPos = GetComponent<CPosition>(entity);
                var cExplode = GetComponent<CExplodeAfterDuration>(entity);

                var rounded = cPos.Position.Rounded();

                var halfWidth = cExplode.Width / 2f;
                var floorWidth = Mathf.FloorToInt(halfWidth);
                var ceilWidth = Mathf.CeilToInt(halfWidth);

                var halfLength = cExplode.Length / 2f;
                var floorLength = Mathf.FloorToInt(halfLength);
                var ceilLength = Mathf.CeilToInt(halfLength);

                for (int x = -floorWidth; x <= ceilWidth; x++)
                {
                    for (int y = -floorLength; y <= ceilLength; y++)
                    {
                        var tilePos = rounded + cPos.Right(x) + cPos.Forward(y);
                        if (!Bounds.Contains(tilePos))
                            continue;

                        var currentTile = GetTile(tilePos);
                        if (currentTile.Type == RoomType.NoRoom)
                            continue;

                        // Walls
                        if (y != ceilLength)
                            TryDestroyWall(currentTile, GetTile(tilePos - cPos.Right(1)));
                        if (x != ceilWidth)
                            TryDestroyWall(currentTile, GetTile(tilePos - cPos.Forward(1)));

                        if (y == ceilLength || x == ceilWidth || x == 0 || y == 0)
                            continue;

                        // Appliances
                        if (!cExplode.DestroyAppliances)
                            continue;

                        var occupant = GetOccupant(tilePos);
                        if (Has<CAppliance>(occupant) && 
                            !Has<CApplianceTable>(occupant) && !Has<CApplianceHostStand>(occupant) && !Has<CApplianceChair>(occupant))
                            EntityManager.DestroyEntity(occupant);
                    }
                }

                CParticleEvent.Create(EntityManager, ParticleEvent.Explosion, rounded);
                CSoundEvent.Create(EntityManager, ExplosionSoundEvent);

                EntityManager.DestroyEntity(entity);

                Set<SRebuildReachability>();
            }
        }

        private void TryDestroyWall(CLayoutRoomTile from, CLayoutRoomTile to)
        {
            if (this.GetTargetableFeature(from, to, out Entity wall))
            {
                Set<CRemovedWall>(wall);
                if (Has<CHatch>(wall))
                {
                    EntityManager.RemoveComponent<CHatch>(wall);
                }
            }
        }
    }
}

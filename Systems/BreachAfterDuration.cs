using Kitchen;
using Kitchen.Layouts;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class BreachAfterDuration : GameSystemBase
    {
        EntityQuery Bombs;
        protected override void Initialise()
        {
            base.Initialise();
            Bombs = GetEntityQuery(typeof(CBreachAfterDuration), typeof(CPosition), typeof(CTakesDuration));
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
                var cBreach = GetComponent<CBreachAfterDuration>(entity);

                for (int i2 = -cBreach.DestroyDistance; i2 < cBreach.DestroyDistance + 1; i2++)
                {
                    var ToHatch = Mathf.Abs(i2) >= cBreach.HatchDistance;
                    DestroyBetween(cPos.Position + cPos.Right(i2), cPos.ForwardPosition + cPos.Right(i2), ToHatch);

                    if (i2 == -cBreach.DestroyDistance)
                        continue;

                    var HatchBetween = Mathf.Ceil(Mathf.Abs(i2 + i2 - 1) * 0.5f) >= cBreach.HatchDistance + 1;
                    DestroyBetween(cPos.Position + cPos.Right(i2), cPos.Position + cPos.Right(i2 - 1), HatchBetween);
                }

                CParticleEvent.Create(EntityManager, ParticleEvent.Explosion, cPos);
                CSoundEvent.Create(EntityManager, ExplosionSoundEvent);

                EntityManager.DestroyEntity(entity);

                Set<SRebuildReachability>();
            }
        }

        private void DestroyBetween(Vector3 from, Vector3 to, bool hatch)
        {
            if (!Bounds.Contains(from) || !Bounds.Contains(to))
                return;

            var currentTile = GetTile(from);
            var toTile = GetTile(to);

            if (currentTile.Type == RoomType.NoRoom || toTile.Type == RoomType.NoRoom)
                return;

            if (this.GetTargetableFeature(currentTile, toTile, out var wall))
            {
                if (!hatch)
                {
                    Set<CRemovedWall>(wall);
                    if (Has<CHatch>(wall))
                    {
                        EntityManager.RemoveComponent<CHatch>(wall);
                    }
                }
                else if (!Has<CReaching>(wall))
                {
                    Set<CReaching>(wall);
                    Set<CHatch>(wall);
                }
            }
        }
    }
}

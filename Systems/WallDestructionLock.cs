using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(DurationLocks))]
    public class WallDestructionLock : GameSystemBase
    {
        private const int DestroyWallDuration = 15;

        private EntityQuery Walls;
        protected override void Initialise()
        {
            base.Initialise();
            Walls = GetEntityQuery(typeof(CTakesDuration), typeof(CTargetableWall), typeof(CWallTargetedBy));
        }

        protected override void OnUpdate()
        {
            using (var entities = Walls.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var buffer = GetBuffer<CWallTargetedBy>(entity);

                    bool anyOnFire = false;
                    float total = DestroyWallDuration;
                    for (int i2 = buffer.Length - 1; i2 >= 0; i2--)
                    {
                        var interactor = buffer[i2].Interactor;
                        if (!Require(interactor, out CDestructive cDestructive) || cDestructive.Target != entity ||
                            Has<CIsInactive>(interactor))
                        {
                            buffer.RemoveAt(i2);
                            continue;
                        }

                        anyOnFire |= Has<CIsOnFire>(interactor);

                        total /= cDestructive.Multiplier;
                    }

                    var cDuration = GetComponent<CTakesDuration>(entity);
                    cDuration.IsLocked = buffer.IsEmpty || anyOnFire;

                    if (!cDuration.IsLocked)
                    {
                        total /= buffer.Length;
                        if (total != cDuration.Total)
                        {
                            cDuration.Remaining = cDuration.Remaining / cDuration.Total * total;
                            cDuration.Total = total;
                        }
                    }
                    Set(entity, cDuration);
                }
            }
        }
    }
}

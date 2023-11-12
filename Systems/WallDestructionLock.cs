using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(DurationLocks))]
    public class WallDestructionLock : GameSystemBase
    {
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

                    bool isRemoved = Has<CRemovedWall>(entity);

                    int incapableInteractors = 0;
                    float total = 10f;
                    for (int i2 = buffer.Length - 1; i2 >= 0; i2--)
                    {
                        var interactor = buffer[i2].Interactor;
                        if (isRemoved || 
                            !Require(interactor, out CDestructive cDestructive) || cDestructive.Target != entity ||
                            Has<CIsInactive>(interactor))
                        {
                            buffer.RemoveAt(i2);
                            continue;
                        }

                        if (Has<CIsOnFire>(interactor))
                        {
                            incapableInteractors++;
                            continue;
                        }

                        total /= cDestructive.Multiplier;
                    }

                    var cDuration = GetComponent<CTakesDuration>(entity);
                    cDuration.IsLocked = buffer.IsEmpty || buffer.Length - incapableInteractors <= 0 || isRemoved;

                    if (!cDuration.IsLocked)
                    {
                        if (Has<CPreventUse>(entity))
                            EntityManager.RemoveComponent<CPreventUse>(entity);

                        total /= buffer.Length - incapableInteractors;
                        if (total != cDuration.Total)
                        {
                            cDuration.Remaining = cDuration.Remaining / cDuration.Total * total;
                            cDuration.Total = total;
                        }
                    }
                    else
                        Set<CPreventUse>(entity);

                    Set(entity, cDuration);
                }
            }
        }

    }
}

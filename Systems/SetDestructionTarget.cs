using Kitchen;
using Kitchen.Layouts;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenRenovation.Systems
{
    [UpdateBefore(typeof(UpdateWallAfterDuration))]
    public class SetDestructionTarget : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CDestructive), typeof(CPosition),
                ComponentType.Exclude<CIsInactive>(), ComponentType.Exclude<CIsOnFire>());
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var destructives = Query.ToComponentDataArray<CDestructive>(Allocator.Temp);
            using var positions = Query.ToComponentDataArray<CPosition>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var cDest = destructives[i];
                var cPos = positions[i];
                var entity = entities[i];
                if (cDest.Target != Entity.Null)
                {
                    if ((!Has<CTargetableWall>(cDest.Target) && !Has<CAppliance>(cDest.Target)) ||
                        Has<CRemovedWall>(cDest.Target))
                    {
                        cDest.Target = Entity.Null;
                        cDest.TargetPosition = Vector3.right * 100;
                        Set(entity, cDest);
                    }
                    continue;
                }

                var rounded = cPos.Position.Rounded();
                if ((cPos.Position - rounded).Chebyshev() > 0.1f)
                    continue;

                var forward = rounded - cPos.Forward(1f);
                var cO = GetOccupant(rounded);
                var fO = GetOccupant(forward);

                var cT = GetTile(rounded);
                var fT = GetTile(forward);

                if ((!this.TryGetFeature(rounded, forward, out var feature) || !feature.Type.IsDoor() || fO != Entity.Null || cO != Entity.Null) &&
                    this.GetTargetableFeature(cT, fT, out var target) && Has<CTargetableWall>(target) && !Has<CRemovedWall>(target))
                {
                    if (!cDest.DestroyToWall && (feature.Type.IsReaching() || Has<CReaching>(target)))
                    {
                        Set<CIsInactive>(entity);
                        continue;
                    }

                    var buffer = GetBuffer<CWallTargetedBy>(target);
                    buffer.Add(new()
                    {
                        Interactor = entity,
                        Hatch = !cDest.DestroyToWall,
                        Destroy = true
                    });

                    cDest.TargetPosition = rounded - cPos.Forward(cDest.WallOffset);
                    cDest.Target = target;
                    Set(entity, cDest);
                    continue;
                }

                if (!cDest.TargetAppliances)
                    continue;

                if (Has<CAppliance>(fO) && !Has<CAllowMobilePathing>(fO))
                {
                    if (Has<CApplianceChair>(fO) || Has<CApplianceTable>(fO) || Has<CApplianceHostStand>(fO))
                    {
                        Set<CIsInactive>(entity);
                        continue;
                    }

                    cDest.TargetPosition = rounded - cPos.Forward(cDest.ApplianceOffset);
                    cDest.Target = fO;
                    Set(entity, cDest);
                    continue;
                }
            }
        }
    }
}

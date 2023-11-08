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
                    if ((!Has<CTargetableWall>(cDest.Target) && !Has<CAppliance>(cDest.Target)))
                    {
                        cDest.Target = Entity.Null;
                        cDest.TargetPosition = Vector3.right * 100;
                        Set(entity, cDest);
                    }
                    continue;
                }

                if (cDest.Target == Entity.Null)
                {
                    cDest.TargetPosition = Vector3.right * 100;
                    Set(entity, cDest);
                }

                var rounded = cPos.Position.Rounded();
                if ((cPos.Position - rounded).Chebyshev() > 0.1f)
                    continue;

                var forward = rounded - cPos.Forward(1f);
                if ((!this.TryGetFeature(rounded, forward, out var feature) || !feature.Type.IsDoor()) &&
                    this.GetTargetableFeature(GetTile(rounded), GetTile(forward), out var target) && Has<CTargetableWall>(target) && !Has<CRemovedWall>(target))
                {
                    if (!cDest.DestroyToWall && (feature.Type.IsReaching() || Has<CReaching>(target)))
                    {
                        Set<CIsInactive>(entity);
                        continue;
                    }

                    DynamicBuffer<CWallTargetedBy> buffer;
                    if (!RequireBuffer(target, out buffer))
                        buffer = EntityManager.AddBuffer<CWallTargetedBy>(target);

                    buffer.Add(new()
                    {
                        Interactor = entity,
                        Hatch = !cDest.DestroyToWall,
                        Destroy = true,
                        Create = false
                    });

                    cDest.TargetPosition = rounded - cPos.Forward(cDest.WallOffset);
                    cDest.Target = target;
                    Set(entity, cDest);
                    continue;
                }

                if (!cDest.TargetAppliances || false) // replace false with preference
                    continue;

                var occupant = GetOccupant(forward);
                if (Has<CAppliance>(occupant) && !Has<CAllowMobilePathing>(occupant))
                {
                    cDest.TargetPosition = rounded - cPos.Forward(cDest.ApplianceOffset);
                    cDest.Target = occupant;
                    Set(entity, cDest);
                    continue;
                }
            }
        }
    }
}

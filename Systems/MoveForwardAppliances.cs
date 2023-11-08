using Kitchen;
using Kitchen.Layouts;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class MoveForwardAppliances : DaySystem
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(
                typeof(CForwardMobile), typeof(CMobileBase), typeof(CPosition), 
                ComponentType.Exclude<CIsInactive>(), ComponentType.Exclude<CIsOnFire>());
        }

        protected override void OnUpdate()
        {
            using (var entities = Query.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var cForward = GetComponent<CForwardMobile>(entity);
                    var cBase = GetComponent<CMobileBase>(entity);
                    var cPosition = GetComponent<CPosition>(entity);

                    if (cForward.MaxDistance - (cPosition.Position - cBase.Start).Chebyshev() < 0.1f)
                    {
                        Set<CIsInactive>(entity);
                        continue;
                    }

                    if (Require(entity, out CDestructive cDestructive) && cDestructive.Target != Entity.Null && (cPosition.Position - cDestructive.TargetPosition).Chebyshev() < 0.1f)
                        continue;

                    var rounded = cPosition.Position.Rounded();

                    if ((cPosition.Position - rounded).Chebyshev() < 0.1f)
                    {
                        var forward = rounded + cPosition.Forward(-1f);
                        var occupant = GetOccupant(forward);

                        if (occupant != Entity.Null && Has<CAppliance>(occupant) && 
                            (((!cForward.IgnoreAppliances || !PrefManager.Get<bool>("DestroyAppliance")) && !Has<CAllowMobilePathing>(occupant)) || 
                            Has<CApplianceChair>(occupant) || Has<CApplianceTable>(occupant) || Has<CApplianceHostStand>(occupant)))
                        {
                            Set<CIsInactive>(entity);
                            continue;
                        }

                        var cT = GetTile(rounded);
                        var fT = GetTile(forward);

                        if (cT.Type == RoomType.NoRoom || fT.Type == RoomType.NoRoom || (!cForward.IgnoreWalls &&
                            (!this.TryGetFeature(rounded, forward, out var feature) || !feature.Type.IsDoor()) &&
                            this.GetTargetableFeature(cT, fT, out var _)))
                        {
                            Set<CIsInactive>(entity);
                            continue;
                        }
                    }

                    cPosition.Position -= cPosition.Forward(Mathf.Min(Time.DeltaTime * cForward.Speed * 0.5f, cForward.Speed));
                    Set(entity, cPosition);
                }
            }
        }
    }
}

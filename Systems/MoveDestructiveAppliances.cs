using Kitchen;
using Kitchen.Layouts;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class MoveDestructiveAppliances : GameSystemBase
    {
        private EntityQuery Mobiles;
        private EntityQuery WallTargets;
        protected override void Initialise()
        {
            base.Initialise();
            Mobiles = GetEntityQuery(
                typeof(CDestructiveAppliance), typeof(CLinkedMobileBase), typeof(CPosition), 
                ComponentType.Exclude<CDisableAutomation>()
                );
            WallTargets = GetEntityQuery(typeof(CPosition), typeof(CTargetableWall));
        }

        protected override void OnUpdate()
        {
            using var entities = Mobiles.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var forwardAppliance = GetComponent<CDestructiveAppliance>(entity);
                var atTarget = math.abs(forwardAppliance.TargetDistance - forwardAppliance.CurrentDistance) < 0.05f;
                if (forwardAppliance.CompletedTask || 
                    (atTarget && forwardAppliance.DestructionTarget != Entity.Null) || 
                    !Require(GetComponent<CLinkedMobileBase>(entity), out CPosition basePos))
                    continue;

                // Destination arrival
                if (math.abs(forwardAppliance.TileRange - forwardAppliance.CurrentDistance) < 0.01f)
                {
                    DisableMobile(entity, forwardAppliance);
                    continue;
                }

                // Targeting
                if (atTarget)
                {
                    var tileDist = Mathf.RoundToInt(forwardAppliance.CurrentDistance);
                    var forwardPos = (basePos.Position - basePos.Forward(tileDist + 1f)).Rounded();
                    var currentPos = (basePos.Position - basePos.Forward(tileDist)).Rounded();

                    forwardAppliance.TargetDistance = tileDist + .8f;

                    if (GetTile(forwardPos).Type == RoomType.NoRoom)
                    {
                        DisableMobile(entity, forwardAppliance);
                        continue;
                    }

                    bool blocked = false;
                    bool isAppliance = false;

                    float totalTime = 0;

                    // Appliance targetting
                    var occupant = GetOccupant(forwardPos);
                    if (occupant != Entity.Null && !Has<CAllowMobilePathing>(occupant) && !Has<CMustHaveWall>(occupant) && Has<CAppliance>(occupant))
                    {
                        if (Has<CApplianceTable>(occupant) || Has<CApplianceChair>(occupant) || Has<CImmovable>(occupant) || 
                            Has<CApplianceHostStand>(occupant))
                        {
                            DisableMobile(entity, forwardAppliance);
                            continue;
                        }

                        blocked = !forwardAppliance.DestroysAppliances;

                        totalTime = forwardAppliance.DestroyApplianceTime;

                        forwardAppliance.TargetDistance = tileDist + 0.15f;
                        forwardAppliance.DestructionTarget = occupant;

                        isAppliance = true;
                    }

                    // Wall targetting
                    var hasFeature = TryGetFeature(currentPos, forwardPos, out var feature);
                    if (GetTile(currentPos).RoomID != GetTile(forwardPos).RoomID && 
                        (!hasFeature || !feature.Type.IsDoor() || occupant != Entity.Null || GetOccupant(currentPos) != Entity.Null) &&
                        GetTargetableFeature(currentPos, forwardPos, entity, out var targetedWall))
                    {
                        blocked = !forwardAppliance.DestroysWalls;

                        totalTime = forwardAppliance.DestroyWallTime;

                        forwardAppliance.TargetDistance = tileDist;
                        forwardAppliance.DestructionTarget = targetedWall;

                        isAppliance = false;
                    }

                    // Blocked
                    if (blocked)
                    {
                        DisableMobile(entity, forwardAppliance);
                        continue;
                    }

                    if (totalTime > 0 && Require(entity, out CTakesDuration cDuration))
                    {
                        cDuration.Total = totalTime;
                        Set(entity, cDuration);
                        if (Require(entity, out CDisplayDuration cDisplay))
                        {
                            cDisplay.IsBad = isAppliance;
                            Set(entity, cDisplay);
                        }
                    }
                }

                var pos = GetComponent<CPosition>(entity);

                // Moving
                forwardAppliance.CurrentDistance += Time.DeltaTime * forwardAppliance.Speed * 0.5f;
                pos.Position = basePos.Position - basePos.Forward(forwardAppliance.CurrentDistance);

                Set(entity, forwardAppliance);
                Set(entity, pos);
            }
        }

        private void DisableMobile(Entity mobile, CDestructiveAppliance component)
        {
            component.CompletedTask = true;
            component.DestructionTarget = Entity.Null;
            Set(mobile, component);
        }

        private bool TryGetFeature(Vector3 from, Vector3 to, out CLayoutFeature feature)
        {
            feature = default;
            var buffer = GetBuffer<CLayoutFeature>(GetSingletonEntity<SLayout>());
            for (int i = 0; i < buffer.Length; i++)
            {
                var checkedFeature = buffer[i];
                if ((from.IsSameTile(checkedFeature.Tile1) && to.IsSameTile(checkedFeature.Tile2)) || 
                    (from.IsSameTile(checkedFeature.Tile2) && to.IsSameTile(checkedFeature.Tile1)))
                {
                    feature = checkedFeature;
                    return true;
                }
            }
            return false;
        }

        private bool GetTargetableFeature(Vector3 from, Vector3 to, Entity source, out Entity entity)
        {
            using var walls = WallTargets.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < walls.Length; ++i)
            {
                var wall = walls[i];
                var target = GetComponent<CTargetableWall>(wall);
                if ((from.IsSameTile(target.Tile1) && to.IsSameTile(target.Tile2)) ||
                    (from.IsSameTile(target.Tile2) && to.IsSameTile(target.Tile1)))
                {
                    entity = wall;
                    return !Has<CDestroyedWall>(wall);
                }
            }
            entity = EntityManager.CreateEntity();
            Set<CPosition>(entity, to + ((to - from).normalized * 0.5f));
            Set(entity, new CTargetableWall { Tile1 = from, Tile2 = to });
            return true;
        }
    }
}

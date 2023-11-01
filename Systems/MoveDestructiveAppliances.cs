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
                    forwardAppliance.CompletedTask = true;
                    Set(entity, forwardAppliance);
                    continue;
                }

                // Targeting
                if (atTarget)
                {
                    var tileDist = Mathf.RoundToInt(forwardAppliance.CurrentDistance);
                    var forwardPos = (basePos.Position - basePos.Forward(tileDist + 1f)).Rounded();
                    var currentPos = (basePos.Position - basePos.Forward(tileDist)).Rounded();

                    forwardAppliance.TargetDistance = tileDist + 1f;

                    if (GetTile(forwardPos).Type == RoomType.NoRoom)
                    {
                        forwardAppliance.CompletedTask = true;
                        Set(entity, forwardAppliance);
                        continue;
                    }

                    bool blocked = false;

                    float totalTime = 0;

                    // Appliance targetting
                    var occupant = GetOccupant(forwardPos);
                    if (occupant != Entity.Null && !Has<CAllowMobilePathing>(occupant) && !Has<CMustHaveWall>(occupant) && Has<CAppliance>(occupant))
                    {
                        LogInfo("Targeted appliance");

                        blocked = !forwardAppliance.DestroysAppliances;

                        totalTime = forwardAppliance.DestroyApplianceTime;

                        forwardAppliance.TargetDistance = tileDist + 0.15f;
                        forwardAppliance.DestructionTarget = occupant;
                    }

                    // Wall targetting
                    var hasFeature = TryGetFeature(currentPos, forwardPos, out var feature);
                    if (GetTile(currentPos).RoomID != GetTile(forwardPos).RoomID && (!hasFeature || !feature.Type.IsDoor()) &&
                        GetTargetableFeature(currentPos, forwardPos, entity, out var targetedWall))
                    {
                        LogInfo("Targeted wall");

                        blocked = !forwardAppliance.DestroysWalls;

                        totalTime = forwardAppliance.DestroyWallTime;

                        forwardAppliance.TargetDistance = tileDist;
                        forwardAppliance.DestructionTarget = targetedWall;
                    }

                    // Blocked
                    if (blocked)
                    {
                        forwardAppliance.CompletedTask = true;
                        Set(entity, forwardAppliance);
                        continue;
                    }

                    if (totalTime > 0 && Require(entity, out CTakesDuration cDuration))
                    {
                        cDuration.Total = totalTime;
                        Set(entity, cDuration);
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

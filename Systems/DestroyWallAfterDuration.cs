using Kitchen;
using KitchenLib.Customs;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    [UpdateAfter(typeof(DestroyApplianceAfterDuration))]
    public class DestroyWallAfterDuration : GameSystemBase
    {
        private EntityQuery Mobiles;
        protected override void Initialise()
        {
            base.Initialise();
            Mobiles = GetEntityQuery(typeof(CDestructiveAppliance), typeof(CTakesDuration));
        }

        protected override void OnUpdate()
        {
            using var entities = Mobiles.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var cDuration = GetComponent<CTakesDuration>(entity);
                var cDestructive = GetComponent<CDestructiveAppliance>(entity);

                Entity target = cDestructive.DestructionTarget;
                if (!cDuration.Active || cDuration.Remaining > 0 || target == Entity.Null || !Require(target, out CTargetableWall cWall))
                    continue;

                cDestructive.DestructionTarget = Entity.Null;
                Set(entity, cDestructive);

                Set<CDestroyedWall>(target);

                Set<SRebuildReachability>();

                CSoundEvent.Create(EntityManager, DestroySoundEvent);

                // Destroy appliances on walls
                DestroyWallAppliance(cWall.Tile1, cWall.Tile2);
                DestroyWallAppliance(cWall.Tile2, cWall.Tile1);
            }
        }

        private void DestroyWallAppliance(Vector3 tile1, Vector3 tile2)
        {
            var appliance = GetOccupant(tile1, KitchenData.OccupancyLayer.Wall);
            if (appliance == Entity.Null && Require(appliance, out CPosition position) &&
                !position.BackwardPosition.IsSameTile(tile2) && !Has<CMustHaveWall>(appliance))
                return;
            EntityManager.DestroyEntity(appliance);
        }
    }
}

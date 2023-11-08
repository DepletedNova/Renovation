using Kitchen;
using Kitchen.Layouts;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using KitchenRenovation.GDOs;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Utility
{
    public static class GenericSystemBaseExt
    {
        internal static EntityQuery Walls { get; set; } = default;
        public static bool GetTargetableFeature(this GenericSystemBase system, CLayoutRoomTile from, CLayoutRoomTile to, out Entity entity)
        {
            entity = default;
            if (Walls == default || 
                from.Type == RoomType.NoRoom || to.Type == RoomType.NoRoom)
                return false;

            var EM = system.EntityManager;
            using (var walls = Walls.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < walls.Length; ++i)
                {
                    var wall = walls[i];
                    var target = EM.GetComponentData<CTargetableWall>(wall);
                    if ((from.Position.IsSameTile(target.Tile1) && to.Position.IsSameTile(target.Tile2)) ||
                        (from.Position.IsSameTile(target.Tile2) && to.Position.IsSameTile(target.Tile1)))
                    {
                        entity = wall;
                        return !EM.HasComponent<CRemovedWall>(wall);
                    }
                }
            }

            if (from.RoomID == to.RoomID)
                return false;

            entity = EM.CreateEntity(typeof(CPosition), typeof(CTargetableWall), typeof(CTakesDuration), typeof(CDisplayDuration));
            EM.SetComponentData<CPosition>(entity, (from.Position - to.Position) * 0.5f + to.Position);
            EM.SetComponentData(entity, new CTargetableWall { Tile1 = from.Position, Tile2 = to.Position });
            EM.SetComponentData(entity, new CTakesDuration
            {
                Total = 30,
                Mode = InteractionMode.Items,
                IsLocked = true,
                PreserveProgress = true
            });
            EM.SetComponentData(entity, new CDisplayDuration
            {
                Process = GetCustomGameDataObject<DestroyWallProcess>().ID,
            });
            EM.AddBuffer<CWallTargetedBy>(entity);

            if (system.TryGetFeature(from.Position, to.Position, out var feature))
            {
                if (feature.Type.IsReaching())
                {
                    EM.AddComponent<CReaching>(entity);
                }
            }

            return true;
        }

        public static bool TryGetFeature(this GenericSystemBase system, Vector3 from, Vector3 to, out CLayoutFeature feature)
        {
            var EM = system.EntityManager;
            feature = default;
            var buffer = EM.GetBuffer<CLayoutFeature>(system.GetSingletonEntity<SLayout>());
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
    }
}

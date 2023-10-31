using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class RebuildReachability : GameSystemBase
    {
        private EntityQuery Destroyed;
        protected override void Initialise()
        {
            base.Initialise();
            Destroyed = GetEntityQuery(new QueryHelper().Any(typeof(CDestroyedWall)).All(typeof(CTargetableWall)));
            RequireSingletonForUpdate<SRebuildReachability>();
        }

        protected override void OnUpdate()
        {
            Clear<SRebuildReachability>();

            using var walls = Destroyed.ToComponentDataArray<CTargetableWall>(Allocator.Temp);
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];

                DynamicBuffer<CLayoutRoomTile> tiles = Tiles;
                for (int i2 = 0; i2 < tiles.Length; i2++)
                {
                    var tile = tiles[i2];
                    if (tile.Position.IsSameTile(wall.Tile1))
                    {
                        LogInfo($"Changing reachability: {tile.Position.x},{tile.Position.z}");
                        var forward = wall.Tile2 - wall.Tile1;
                        tile.Reachability[Mathf.RoundToInt(forward.x), Mathf.RoundToInt(forward.z)] |= true;
                    }
                    else if (tile.Position.IsSameTile(wall.Tile2))
                    {
                        LogInfo($"Changing reachability: {tile.Position.x},{tile.Position.z}");
                        var forward = wall.Tile1 - wall.Tile2;
                        tile.Reachability[Mathf.RoundToInt(forward.x), Mathf.RoundToInt(forward.z)] |= true;
                    }
                    tiles[i2] = tile;
                }
            }
        }
    }
}

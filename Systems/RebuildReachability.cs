using Kitchen;
using KitchenRenovation.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            RequireSingletonForUpdate<SLayout>();
        }

        protected override void OnUpdate()
        {
            Clear<SRebuildReachability>();

            using var walls = Destroyed.ToComponentDataArray<CTargetableWall>(Allocator.Temp);
            var layout = GetSingletonEntity<SLayout>();
            var tiles = GetBuffer<CLayoutRoomTile>(layout);
            var features = GetBuffer<CLayoutFeature>(layout);
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];

                // Direct
                for (int i2 = 0; i2 < tiles.Length; i2++)
                {
                    var tile = tiles[i2];

                    UpdateDirect(ref tile, wall.Tile2, wall.Tile1);
                    UpdateDirect(ref tile, wall.Tile1, wall.Tile2);

                    tiles[i2] = tile;
                }

                // Diagonals
                for (int i2 = 0; i2 < tiles.Length; i2++)
                {
                    var tile = tiles[i2];

                    UpdateForDiagonal(ref tile, wall.Tile1, wall.Tile2);
                    UpdateForDiagonal(ref tile, wall.Tile2, wall.Tile1);
                    UpdateDiagonal(ref tile, wall.Tile1, wall.Tile2);
                    UpdateDiagonal(ref tile, wall.Tile2, wall.Tile1);

                    tiles[i2] = tile;
                }

                // Feature swap
                for (int i2 = 0; i2 < features.Length; i2++)
                {
                    var feature = features[i2];
                    if ((feature.Tile1 == wall.Tile1 && feature.Tile2 == wall.Tile2) || (feature.Tile2 == wall.Tile1 && feature.Tile1 == wall.Tile2))
                    {
                        feature.Type = Kitchen.Layouts.FeatureType.Hatch;
                        features[i2] = feature;
                    }
                }
            }
        }

        public bool IsDiagonal(Vector3 from, Vector3 to) => 
            Mathf.Abs(to.x - from.x) == 1 && Mathf.Abs(to.z - from.z) == 1;

        public void UpdateDirect(ref CLayoutRoomTile tile, Vector3 from, Vector3 to)
        {
            if (tile.Position != from)
                return;

            var forward = to - from;
            tile.Reachability[(int)forward.x, (int)forward.z] = true;
        }

        public void UpdateForDiagonal(ref CLayoutRoomTile tile, Vector3 from, Vector3 to)
        {
            if (tile.Position != from)
                return;

            var offset = to - from;
            var offsetTile = GetTile(to);
            int x = (int)(offset.z != 0 ? offset.z : 0);
            int z = (int)(offset.x != 0 ? offset.x : 0);

            tile.Reachability[(int)offset.x + x, (int)offset.z + z] |=
                offsetTile.Reachability[x, z];
            tile.Reachability[(int)offset.x - x, (int)offset.z - z] |=
                offsetTile.Reachability[-x, -z];
        }

        public void UpdateDiagonal(ref CLayoutRoomTile tile, Vector3 near, Vector3 far)
        {
            if (!IsDiagonal(tile.Position, far))
                return;

            tile.Reachability[(int)(far.x - tile.Position.x), (int)(far.z - tile.Position.z)] |=
                tile.Reachability[(int)(near.x - tile.Position.x), (int)(near.z - tile.Position.z)];
        }


    }
}

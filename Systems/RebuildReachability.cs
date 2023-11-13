using Kitchen;
using Kitchen.Layouts;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class RebuildReachability : GameSystemBase
    {
        private EntityQuery Walls;
        protected override void Initialise()
        {
            base.Initialise();
            Walls = GetEntityQuery(typeof(CTargetableWall));
            RequireSingletonForUpdate<SRebuildReachability>();
            RequireSingletonForUpdate<SLayout>();
        }

        protected override void OnUpdate()
        {
            Clear<SRebuildReachability>();

            using var entities = Walls.ToEntityArray(Allocator.Temp);
            var layout = GetSingletonEntity<SLayout>();
            var tiles = GetBuffer<CLayoutRoomTile>(layout);
            var features = GetBuffer<CLayoutFeature>(layout);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var wall = GetComponent<CTargetableWall>(entity);

                var unblocked = Has<CRemovedWall>(entity) || Has<CReaching>(entity);

                if (!unblocked && !Has<CPlacedWall>(entity))
                    continue;

                // Direct
                for (int i2 = 0; i2 < tiles.Length; i2++)
                {
                    var tile = tiles[i2];

                    UpdateDirect(ref tile, wall.Tile2, wall.Tile1, unblocked);
                    UpdateDirect(ref tile, wall.Tile1, wall.Tile2, unblocked);

                    tiles[i2] = tile;
                }

                // Diagonals
                for (int i2 = 0; i2 < tiles.Length; i2++)
                {
                    var tile = tiles[i2];

                    UpdateForDiagonal(ref tile, wall.Tile1, wall.Tile2, unblocked);
                    UpdateForDiagonal(ref tile, wall.Tile2, wall.Tile1, unblocked);
                    UpdateDiagonal(ref tile, wall.Tile1, wall.Tile2);
                    UpdateDiagonal(ref tile, wall.Tile2, wall.Tile1);

                    tiles[i2] = tile;
                }

                // Feature swap/build
                bool replacedFeature = false;
                for (int i2 = 0; i2 < features.Length; i2++)
                {
                    var feature = features[i2];
                    if ((feature.Tile1 == wall.Tile1 && feature.Tile2 == wall.Tile2) || (feature.Tile2 == wall.Tile1 && feature.Tile1 == wall.Tile2))
                    {
                        feature.Type = FeatureType.Hatch;
                        features[i2] = feature;
                        replacedFeature = true;
                        break;
                    }
                }
                if (!replacedFeature)
                {
                    features.Add(new CLayoutFeature
                    {
                        Tile1 = wall.Tile1,
                        Tile2 = wall.Tile2,
                        Type = FeatureType.Hatch
                    });
                }
            }
        }

        public void UpdateDirect(ref CLayoutRoomTile tile, Vector3 from, Vector3 to, bool unblocked)
        {
            if (tile.Position != from)
                return;

            tile.Reachability[(int)(to.x - from.x), (int)(to.z - from.z)] = unblocked;
        }

        public void UpdateForDiagonal(ref CLayoutRoomTile tile, Vector3 from, Vector3 to, bool unblocked)
        {
            if (tile.Position != from)
                return;

            var offset = (to - from).ToInt();

            var z1Tile = GetTile(new Vector3(from.x + offset.z, 0, from.z + offset.x));
            var z1Reach =
                z1Tile.Reachability[offset.x, offset.z] &&
                z1Tile.Reachability[-offset.z, -offset.x];

            var z2Tile = GetTile(new Vector3(from.x - offset.z, 0, from.z - offset.x));
            var z2Reach =
                z2Tile.Reachability[offset.x, offset.z] &&
                z2Tile.Reachability[offset.z, offset.x];

            if (unblocked)
            {
                var toTile = GetTile(to);
                z1Reach |= toTile.Reachability[offset.z, offset.x];
                z2Reach |= toTile.Reachability[-offset.z, -offset.x];
            }

            tile.Reachability[offset.x + offset.z, offset.z + offset.x] = z1Reach;
            tile.Reachability[offset.x - offset.z, offset.z - offset.x] = z2Reach;
        }

        public void UpdateDiagonal(ref CLayoutRoomTile tile, Vector3 near, Vector3 far)
        {
            if (!tile.Position.IsDiagonal(far))
                return;

            var offset = (far - near).ToInt();
            var farTile = GetTile(far);

            tile.Reachability[(int)(far.x - tile.Position.x), (int)(far.z - tile.Position.z)] =
                (tile.Reachability[offset.z, offset.x] && farTile.Reachability[-offset.x, -offset.z]) ||
                (tile.Reachability[offset.x, offset.z] && farTile.Reachability[-offset.z, -offset.x]);
        }

    }
}

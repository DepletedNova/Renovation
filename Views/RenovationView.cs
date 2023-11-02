using Kitchen;
using KitchenRenovation.Components;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class RenovationView : UpdatableObjectView<RenovationView.ViewData>
    {
        public static LayoutView Layout;

        // Cached
        public List<GameObject> DestroyedWalls = new();

        // Prefabs
        public GameObject LinerPrefab;
        public GameObject HatchPrefab;

        private ViewData Data = default;
        protected override void UpdateData(ViewData data)
        {
            if (Layout == null || !Data.IsChangedFrom(data))
                return;

            Data = data;

            var builder = Layout.Builder;

            // Walls
            for (int i = builder.Walls.Count - 1; i >= 0; i--)
            {
                var wall = builder.Walls[i];
                var tile1 = wall.Tile1.ToWorld();
                var tile2 = wall.Tile2.ToWorld();

                if (wall.Collider == null)
                    continue;

                // Destroyed
                if (Data.DestroyedWalls.Any(p => (p.Item1 == tile1 && p.Item2 == tile2) ||
                        (p.Item2 == tile1 && p.Item1 == tile2)))
                {
                    Destroy(wall.Collider.gameObject);
                    builder.Walls.RemoveAt(i);
                    continue;
                }
            }

            // Doors
            for (int i = builder.Doors.Count - 1; i >= 0; i--)
            {
                var door = builder.Doors[i];
                var tile1 = door.Tile1.ToWorld();
                var tile2 = door.Tile2.ToWorld();

                if (door.DoorController == null)
                    continue;
                
                // Destroyed
                if (Data.DestroyedWalls.Any(p => (p.Item1 == tile1 && p.Item2 == tile2) ||
                        (p.Item2 == tile1 && p.Item1 == tile2)))
                {
                    Destroy(door.DoorController.gameObject);
                    builder.Doors.RemoveAt(i);
                    continue;
                }
            }

        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public List<ValueTuple<Vector3, Vector3>> DestroyedWalls;

            public bool IsChangedFrom(ViewData check) =>
                !DestroyedWalls.IsEqual(check.DestroyedWalls);
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery ModifiedWalls;
            private EntityQuery Views;
            protected override void Initialise()
            {
                base.Initialise();
                ModifiedWalls = GetEntityQuery(new QueryHelper()
                    .All(typeof(CTargetableWall))
                    .Any(typeof(CDestroyedWall)));

                Views = GetEntityQuery(typeof(SRenovation), typeof(CLinkedView));

                RequireForUpdate(Views);
                RequireSingletonForUpdate<SLayout>();
            }

            protected override void OnUpdate()
            {
                if (ModifiedWalls.IsEmpty)
                    return;

                List<ValueTuple<Vector3, Vector3>> destroyed = new();
                using (var walls = ModifiedWalls.ToEntityArray(Allocator.Temp))
                {
                    for (int i = 0; i < walls.Length; i++)
                    {
                        if (!Has<CDestroyedWall>(walls[i]))
                            continue;

                        var wall = GetComponent<CTargetableWall>(walls[i]);
                        destroyed.Add((wall.Tile1, wall.Tile2));
                    }
                }

                using (var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    for (int i = 0; i < views.Length; i++)
                    {
                        SendUpdate(views[i], new ViewData
                        {
                            DestroyedWalls = destroyed,
                        });
                    }
                }

            }
        }
    }
}

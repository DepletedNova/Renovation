using Kitchen;
using KitchenLib.Utils;
using KitchenRenovation.Components;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Views
{
    public class RenovationView : UpdatableObjectView<RenovationView.ViewData>
    {
        public static LayoutView Layout;

        // Cached
        public List<Liner> Liners = new();
        public List<Hatch> Hatches = new();
        public List<Doorstop> Doorstops = new();

        // Prefabs
        [SerializeField] public GameObject LinerPrefab;
        [SerializeField] public GameObject DoorstopPrefab;
        [SerializeField] public GameObject HatchPrefab;

        private ViewData Data = default;
        protected override void UpdateData(ViewData data)
        {
            if (Layout == null || !Data.IsChangedFrom(data))
                return;

            Data = data;

            for (int i = Data.WallModifications.Count - 1; i >= 0; i--)
            {
                var mod = Data.WallModifications[i];

                var Tile1 = mod.Item1;
                var Tile2 = mod.Item2;

                // Remove unused
                if (!TryRemoveAtPosition(Tile1, Tile2, mod.Item3))
                    continue;

                if (mod.Item3 == ChangeType.Removed)
                {
                    // Removed
                    GetWallTransforms(Tile1, Tile2, out var pos, out var rot);
                    var gameObject = Instantiate(LinerPrefab);
                    gameObject.transform.SetParent(transform, false);
                    gameObject.transform.position = pos;
                    gameObject.transform.rotation = rot;

                    Liners.Add(new()
                    {
                        LinerObject = gameObject,
                        Tile1 = Tile1,
                        Tile2 = Tile2,
                    });
                } else
                {
                    // Hatch
                    GetWallTransforms(Tile1, Tile2, out var pos, out var rot);
                    var gameObject = Instantiate(HatchPrefab);
                    gameObject.transform.SetParent(transform, false);
                    gameObject.transform.position = pos;
                    gameObject.transform.rotation = rot;

                    Hatches.Add(new()
                    {
                        HatchObject = gameObject,
                        Tile1 = Tile1,
                        Tile2 = Tile2,
                    });
                }
            }

            // Clear unused doorstops
            for (int i = Doorstops.Count - 1; i >= 0; i--)
            {
                var doorstop = Doorstops[i];
                if (!Layout.Builder.Doors.Any(p => p.Tile1.ToWorld() == doorstop.Tile || p.Tile2.ToWorld() == doorstop.Tile) ||
                    !Data.Doorstops.Any(p => p == doorstop.Tile))
                {
                    Destroy(doorstop.Attached);
                    Doorstops.RemoveAt(i);
                }
            }

            // Create or Update doorstops
            for (int i = 0; i < Layout.Builder.Doors.Count; i++)
            {
                var door = Layout.Builder.Doors[i];

                if (door.DoorController == null || door.DoorController.IsExternal || door.MoveAtNight)
                    continue;

                var openerIndex = Data.Doorstops.FindIndex(p => p == door.Tile1.ToWorld() || p == door.Tile2.ToWorld());
                var shouldOpen = openerIndex != -1 && !door.IsCurrentlyDisabled;

                var doorIndex = Doorstops.FindIndex(p => p.Door.Equals(door));
                if (doorIndex != -1)
                {
                    if (shouldOpen)
                        return;
                    else
                        Doorstops.RemoveAt(doorIndex);
                }

                var controller = door.DoorController;

                if (shouldOpen)
                {
                    Quaternion defaultRot = (Quaternion)ReflectionUtils.GetField<DoorController>("DoorDefaultRotation").GetValue(controller);
                    controller.ResetAngle();
                    var offset = Data.Doorstops[openerIndex] - door.DoorGameObject.transform.position;
                    var theta = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg - (defaultRot.eulerAngles.y + door.DoorGameObject.transform.rotation.eulerAngles.y);
                    controller.TargetPosition = Math.Abs(theta) % 360 < 180 ? 135 : -135;

                    var gameObject = Instantiate(DoorstopPrefab);
                    gameObject.transform.SetParent(transform, false);
                    gameObject.transform.position = door.DoorGameObject.transform.position;
                    gameObject.transform.rotation = door.DoorGameObject.transform.rotation;

                    gameObject.GetComponent<FixedJoint>().connectedBody = controller.Hinge.gameObject.GetComponent<Rigidbody>();

                    Doorstops.Add(new Doorstop
                    {
                        Attached = gameObject,
                        Door = door,
                        Tile = Data.Doorstops[openerIndex]
                    });
                }

                controller.SetSpring(shouldOpen);
                controller.SetCollision(!shouldOpen);
            }
        }

        private bool TryRemoveAtPosition(Vector3 Tile1, Vector3 Tile2, ChangeType change)
        {
            var builder = Layout.Builder;

            // Walls
            var wallIndex = builder.Walls.FindIndex(p => 
                (p.Tile1.ToWorld() == Tile1 && p.Tile2.ToWorld() == Tile2) || 
                (p.Tile2.ToWorld() == Tile1 && p.Tile1.ToWorld() == Tile2));
            if (wallIndex != -1)
            {
                var wall = builder.Walls[wallIndex];
                if (wall.Collider != null)
                {
                    Destroy(wall.Collider.gameObject);
                    builder.Walls.RemoveAt(wallIndex);
                }
            }

            // Doors
            var doorIndex = 
                builder.Doors.FindIndex(p => (p.Tile1.ToWorld() == Tile1 && p.Tile2.ToWorld() == Tile2) || (p.Tile2.ToWorld() == Tile1 && p.Tile1.ToWorld() == Tile2));
            if (doorIndex != -1)
            {
                var door = builder.Doors[doorIndex];
                if (door.DoorController != null)
                {
                    Destroy(door.DoorController.gameObject);
                    builder.Walls.RemoveAt(wallIndex);
                }
            }

            // Hatches
            var hatchIndex = Hatches.FindIndex(p => (p.Tile1 == Tile1 && p.Tile2 == Tile2) || (p.Tile2 == Tile1 && p.Tile1 == Tile2));
            if (hatchIndex != -1)
            {
                if (change == ChangeType.Hatch)
                    return false;

                var hatch = Hatches[hatchIndex];
                Destroy(hatch.HatchObject);
                Hatches.RemoveAt(hatchIndex);
            }

            // Removed
            var removeIndex = Liners.FindIndex(p => (p.Tile1 == Tile1 && p.Tile2 == Tile2) || (p.Tile2 == Tile1 && p.Tile1 == Tile2));
            if (removeIndex != -1)
            {
                if (change == ChangeType.Removed)
                    return false;

                var remove = Liners[removeIndex];
                Destroy(remove.LinerObject);
                Liners.RemoveAt(removeIndex);
            }

            return true;
        }

        public void GetWallTransforms(Vector3 Tile1, Vector3 Tile2, out Vector3 Position, out Quaternion Rotation)
        {
            var offset = Tile2 - Tile1;
            Position = offset * 0.5f + Tile1;
            Rotation = Quaternion.LookRotation(offset, Vector3.up);
        }

        public struct Liner
        {
            public Vector3 Tile1;
            public Vector3 Tile2;
            public GameObject LinerObject;
        }

        public struct Hatch
        {
            public Vector3 Tile1;
            public Vector3 Tile2;
            public GameObject HatchObject;
        }

        public struct Doorstop
        {
            public Door Door;
            public GameObject Attached;
            public Vector3 Tile;
        }

        public enum ChangeType
        {
            Removed,
            Hatch,
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public List<ValueTuple<Vector3, Vector3, ChangeType>> WallModifications;
            [Key(2)] public List<Vector3> Doorstops;

            public bool IsChangedFrom(ViewData check) =>
                !WallModifications.IsEqual(check.WallModifications) || !Doorstops.IsEqual(check.Doorstops);
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery ModifiedWalls;
            private EntityQuery Doorstops;

            private EntityQuery Views;
            protected override void Initialise()
            {
                base.Initialise();

                ModifiedWalls = GetEntityQuery(typeof(CTargetableWall));
                Doorstops = GetEntityQuery(typeof(CDoorstop), typeof(CPosition));

                Views = GetEntityQuery(typeof(SRenovation), typeof(CLinkedView));

                RequireForUpdate(Views);
                RequireSingletonForUpdate<SLayout>();
            }

            protected override void OnUpdate()
            {
                List<ValueTuple<Vector3, Vector3, ChangeType>> changes = new();
                using (var walls = ModifiedWalls.ToEntityArray(Allocator.Temp))
                {
                    for (int i = 0; i < walls.Length; i++)
                    {
                        var entity = walls[i];
                        var wall = GetComponent<CTargetableWall>(walls[i]);
                        var hatch = Has<CHatch>(entity);
                        if (hatch || Has<CRemovedWall>(entity))
                        {
                            changes.Add((wall.Tile1, wall.Tile2, hatch ? ChangeType.Hatch : ChangeType.Removed));
                        }
                    }
                }

                List<Vector3> openers = new();
                if (HasSingleton<SIsDayTime>())
                {
                    using (var doorstops = Doorstops.ToComponentDataArray<CPosition>(Allocator.Temp))
                    {
                        foreach (var stop in doorstops)
                        {
                            openers.Add(stop.Position.Rounded());
                        }
                    }
                }
                
                using (var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    for (int i = 0; i < views.Length; i++)
                    {
                        SendUpdate(views[i], new ViewData
                        {
                            WallModifications = changes,
                            Doorstops = openers,
                        });
                    }
                }

            }
        }
    }
}

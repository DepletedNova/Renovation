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
        public List<Doorstop> Doorstops = new();

        // Prefabs
        [SerializeField] public GameObject LinerPrefab;
        [SerializeField] public GameObject DoorstopPrefab;

        private ViewData Data = default;
        protected override void UpdateData(ViewData data)
        {
            if (Layout == null || !Data.IsChangedFrom(data))
                return;

            Data = data;

            CleanObjects();

            var builder = Layout.Builder;

            ModifyWalls(builder);
            ModifyDoors(builder);
        }

        public void CleanObjects()
        {
            CleanLiners();
            CleanDoorstops();
        }

        public void CleanLiners()
        {
            for (int i = Liners.Count - 1; i >= 0; i--)
            {
                var liner = Liners[i];
                if (Data.Destroyed.Any(p => (p.Item1 == liner.Tile1 && p.Item2 == liner.Tile2) ||
                        (p.Item2 == liner.Tile1 && p.Item1 == liner.Tile2)))
                    continue;

                Destroy(liner.LinerObject);
                Liners.RemoveAt(i);
            }
        }

        public void CleanDoorstops()
        {
            for (int i = Doorstops.Count - 1; i >= 0; i--)
            {
                var stop = Doorstops[i];
                if (Data.Doorstops.Any(p => stop.Tile == p))
                    continue;

                Destroy(stop.Attached);
                Doorstops.RemoveAt(i);
            }
        }

        public void ModifyWalls(LayoutBuilder builder)
        {
            for (int i = builder.Walls.Count - 1; i >= 0; i--)
            {
                var wall = builder.Walls[i];
                if (wall.Collider == null)
                    continue;

                var tile1 = wall.Tile1.ToWorld();
                var tile2 = wall.Tile2.ToWorld();

                if (DestroyWall(tile1, tile2))
                {
                    Destroy(wall.Collider.gameObject);
                    builder.Walls.RemoveAt(i);
                    continue;
                }
            }
        }

        public void ModifyDoors(LayoutBuilder builder)
        {
            for (int i = builder.Doors.Count - 1; i >= 0; i--)
            {
                var door = builder.Doors[i];
                if (door.DoorController == null)
                    continue;

                var tile1 = door.Tile1.ToWorld();
                var tile2 = door.Tile2.ToWorld();

                if (DestroyWall(tile1, tile2))
                {
                    Destroy(door.DoorController.gameObject);
                    builder.Doors.RemoveAt(i);
                    continue;
                }
                OpenDoor(door, tile1, tile2);
            }
        }

        public bool DestroyWall(Vector3 Tile1, Vector3 Tile2)
        {
            if (!Data.Destroyed.Any(p => (p.Item1 == Tile1 && p.Item2 == Tile2) ||
                        (p.Item2 == Tile1 && p.Item1 == Tile2)))
                return false;

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

            return true;
        }

        public void OpenDoor(Door door, Vector3 Tile1, Vector3 Tile2)
        {
            var controller = door.DoorController;
            if (controller.IsExternal || door.MoveAtNight)
                return;

            var OpenerIndex = Data.Doorstops.FindIndex(p => p == Tile1 || p == Tile2);
            var shouldOpen = OpenerIndex != -1;

            var DoorIndex = Doorstops.FindIndex(p => p.Door.Equals(door));
            if (DoorIndex != -1)
            {
                if (shouldOpen)
                    return;
                else
                    Doorstops.RemoveAt(DoorIndex);
            }

            if (door.IsCurrentlyDisabled)
                shouldOpen = false;

            if (shouldOpen)
            {
                Quaternion defaultRot = (Quaternion)ReflectionUtils.GetField<DoorController>("DoorDefaultRotation").GetValue(controller);
                controller.ResetAngle();
                var offset = Data.Doorstops[OpenerIndex] - door.DoorGameObject.transform.position;
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
                    Tile = Data.Doorstops[OpenerIndex]
                });
            }

            controller.SetSpring(shouldOpen);
            controller.SetCollision(!shouldOpen);
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

        public struct Doorstop
        {
            public Door Door;
            public GameObject Attached;
            public Vector3 Tile;
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public List<ValueTuple<Vector3, Vector3>> Destroyed;
            [Key(2)] public List<Vector3> Doorstops;

            public bool IsChangedFrom(ViewData check) =>
                !Destroyed.IsEqual(check.Destroyed) || !Doorstops.IsEqual(check.Doorstops);
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery ModifiedWalls;
            private EntityQuery Doorstops;

            private EntityQuery Views;
            protected override void Initialise()
            {
                base.Initialise();

                ModifiedWalls = GetEntityQuery(new QueryHelper()
                    .All(typeof(CTargetableWall))
                    .Any(typeof(CRemovedWall)));
                Doorstops = GetEntityQuery(typeof(CDoorstop), typeof(CPosition));

                Views = GetEntityQuery(typeof(SRenovation), typeof(CLinkedView));

                RequireForUpdate(Views);
                RequireSingletonForUpdate<SLayout>();
            }

            protected override void OnUpdate()
            {
                List<ValueTuple<Vector3, Vector3>> destroyed = new();
                using (var walls = ModifiedWalls.ToEntityArray(Allocator.Temp))
                {
                    for (int i = 0; i < walls.Length; i++)
                    {
                        if (!Has<CRemovedWall>(walls[i]))
                            continue;

                        var wall = GetComponent<CTargetableWall>(walls[i]);
                        destroyed.Add((wall.Tile1, wall.Tile2));
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
                            Destroyed = destroyed,
                            Doorstops = openers
                        });
                    }
                }

            }
        }
    }
}

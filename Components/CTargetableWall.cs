using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Components
{
    public struct CTargetableWall : IComponentData
    {
        public Vector3 Tile1;
        public Vector3 Tile2;
    }
}

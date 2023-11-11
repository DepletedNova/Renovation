using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Components
{
    public struct CDestructive : IApplianceProperty, IItemProperty
    {
        public bool TargetAppliances;
        public bool DestroyToWall;
        public float Multiplier;

        public Entity Target;
        public Vector3 TargetPosition;

        public float ApplianceOffset;
        public float WallOffset;
    }
}

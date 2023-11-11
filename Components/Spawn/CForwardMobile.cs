using KitchenData;
using UnityEngine;

namespace KitchenRenovation.Components
{
    public struct CForwardMobile : IApplianceProperty
    {
        public float MaxDistance;

        public bool IgnoreWalls;
        public bool IgnoreAppliances;

        public float Speed;
    }
}

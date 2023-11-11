using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace KitchenRenovation.Components
{
    public struct CMobileBase : IApplianceProperty
    {
        public Vector3 Start;
        public Entity Home;
    }
}

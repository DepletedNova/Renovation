using KitchenData;
using Unity.Entities;

namespace KitchenRenovation.Components
{
    public struct CLinkedMobileBase : IApplianceProperty
    {
        public Entity Base;

        public static implicit operator Entity(CLinkedMobileBase component) => component.Base;
        public static explicit operator CLinkedMobileBase(Entity entity) => new CLinkedMobileBase { Base = entity };
    }
}

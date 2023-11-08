using Unity.Entities;

namespace KitchenRenovation.Components
{
    [InternalBufferCapacity(4)]
    public struct CWallTargetedBy : IBufferElementData
    {
        public Entity Interactor;
        public bool Destroy;
        public bool Create;
        public bool Hatch;
    }
}

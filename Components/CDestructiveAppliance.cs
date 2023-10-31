using KitchenData;
using Unity.Entities;

namespace KitchenRenovation.Components
{
    public struct CDestructiveAppliance : IApplianceProperty
    {
        public float TileRange;
        public float Speed;

        public bool DestroysWalls;
        public float DestroyWallTime;

        public bool DestroysAppliances;
        public float DestroyApplianceTime;


        public Entity DestructionTarget;
        public float TargetDistance;
        public float CurrentDistance;
        public bool CompletedTask;
    }
}

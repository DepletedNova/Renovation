using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class InteractDisableMobile : ItemInteractionSystem
    {
        private CDestructiveAppliance cDestructive;
        protected override bool IsPossible(ref InteractionData data) =>
            Require<CDestructiveAppliance>(data.Target, out var comp) && !comp.CompletedTask;

        protected override void Perform(ref InteractionData data)
        {
            cDestructive.CompletedTask = true;
            cDestructive.DestructionTarget = Entity.Null;
            Set(data.Target, cDestructive);
        }
    }
}

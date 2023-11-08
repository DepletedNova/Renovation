using Kitchen;
using KitchenRenovation.Components;

namespace KitchenRenovation.Systems
{
    public class InteractDisable : ItemInteractionSystem
    {
        protected override bool IsPossible(ref InteractionData data) =>
            !Has<CIsInactive>(data.Target) && Has<CInteractDisable>(data.Target);

        protected override void Perform(ref InteractionData data) =>
            Set<CIsInactive>(data.Target);
    }
}

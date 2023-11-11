using Kitchen;
using KitchenRenovation.Components;

namespace KitchenRenovation.Systems
{
    public class InteractDestroysDisabled : ItemInteractionSystem
    {
        protected override bool IsPossible(ref InteractionData data) =>
            Has<CInteractDestroysDisabled>(data.Target) && Has<CIsInactive>();

        protected override void Perform(ref InteractionData data)
        {
            EntityManager.DestroyEntity(data.Target);
        }
    }
}

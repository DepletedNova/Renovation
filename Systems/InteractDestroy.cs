using Kitchen;
using KitchenRenovation.Components;

namespace KitchenRenovation.Systems
{
    public class InteractDestroy : ItemInteractionSystem
    {
        protected override bool IsPossible(ref InteractionData data) =>
            Has<CInteractRemove>(data.Target);

        protected override void Perform(ref InteractionData data)
        {
            EntityManager.DestroyEntity(data.Target);
        }
    }
}

using Kitchen;
using KitchenRenovation.Components;

namespace KitchenRenovation.Systems
{
    public class RemoveInactiveFromDynamite : InteractionSystem
    {
        protected override bool AllowAnyMode => true;
        protected override InteractionType RequiredType => InteractionType.Act;

        protected override bool IsPossible(ref InteractionData data) =>
            Has<CIsInactive>(data.Target) && Require(data.Target, out CDynamite cDynamite) &&
            ((!cDynamite.UseAtNight && Has<SIsDayTime>()) || (cDynamite.UseAtNight && Has<SIsNightTime>()));

        protected override void Perform(ref InteractionData data)
        {
            EntityManager.RemoveComponent<CIsInactive>(data.Target);
            Set<CImmovable>(data.Target);
        }
    }
}

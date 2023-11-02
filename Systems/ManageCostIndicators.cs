using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class ManageCostIndicators : IndicatorManager
    {
        protected override ViewType ViewType => PurchaseView;

        protected override EntityQuery GetSearchQuery() => GetEntityQuery(typeof(CCanBeDailyPurchased), typeof(CPosition));

        protected override bool ShouldHaveIndicator(Entity candidate) =>
            !Has<CHasDailyPurchase>(candidate) && !Has<CHeldBy>(candidate) && !Has<CDisplayDuration>(candidate) && HasSingleton<SIsNightTime>();

        protected override Entity CreateIndicator(Entity source)
        {
            if (!Require(source, out CPosition position) || !Require(source, out CCanBeDailyPurchased purchase))
                return Entity.Null;

            var indicator = base.CreateIndicator(source);
            Set<CPosition>(indicator, position.Position);
            Set(indicator, new CCostIndicator { Cost = purchase.Cost });
            return indicator;
        }
    }
}

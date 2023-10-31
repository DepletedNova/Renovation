using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class ManageCostIndicators : IndicatorManager
    {
        protected override ViewType ViewType => PurchaseView;

        protected override EntityQuery GetSearchQuery() => GetEntityQuery(new QueryHelper()
            .All(typeof(CCanBeDailyPurchased), typeof(CPosition)).None(typeof(CHasDailyPurchase)));

        protected override bool ShouldHaveIndicator(Entity candidate) =>
            !Has<CHasDailyPurchase>(candidate) && !Has<CBeingLookedAt>(candidate) && !Has<CHeldBy>() && !Has<SIsNightTime>();

        protected override Entity CreateIndicator(Entity source)
        {
            var indicator = base.CreateIndicator(source);
            Set<CPosition>(indicator, GetComponent<CPosition>(source).Position);
            Set(indicator, new CCostIndicator { Cost = GetComponent<CCanBeDailyPurchased>(source).Count });
            return indicator;
        }
    }
}

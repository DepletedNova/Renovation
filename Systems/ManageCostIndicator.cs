using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class ManageCostIndicator : IndicatorManager
    {
        protected override ViewType ViewType => PurchaseView;

        protected override EntityQuery GetSearchQuery() => 
            GetEntityQuery(typeof(CPurchaseable), typeof(CTakesDuration), typeof(CPosition));

        protected override bool ShouldHaveIndicator(Entity candidate) =>
            Has<SIsNightTime>() &&
            !Has<CHasPurchase>(candidate) && !Has<CHeldBy>(candidate) &&
            Require(candidate, out CTakesDuration cDuration) && cDuration.Active && (cDuration.Remaining == cDuration.Total || cDuration.Remaining == 0 || cDuration.IsLocked);

        protected override Entity CreateIndicator(Entity source)
        {
            int cost = GetComponent<CPurchaseable>(source).Cost;

            if (Require(source, out CRampingCost cRamping) && Require(out SDay sDay))
            {
                var currentDay = GetSingleton<SDay>().Day;
                if (cRamping.UseBoughtDay && cRamping.MinimumDay == 0)
                {
                    cRamping.MinimumDay = currentDay;
                    Set(source, cRamping);
                }

                var day = currentDay - cRamping.MinimumDay;
                if (day > 0)
                    cost += cRamping.IncreasedCost * (day / cRamping.DayIncrement);
            }

            var entity = base.CreateIndicator(source);
            Set<CPosition>(entity, GetComponent<CPosition>(source).Position);
            Set(entity, new CCostIndicator
            {
                Cost = cost
            });
            return entity;
        }
    }
}

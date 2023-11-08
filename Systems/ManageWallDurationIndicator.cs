using Kitchen;
using KitchenRenovation.Components;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class ManageWallDurationIndicator : IndicatorManager
    {
        protected override ViewType ViewType => ViewType.ProgressView;
        protected override ViewMode ViewMode => ViewMode.World;

        protected override EntityQuery GetSearchQuery() => GetEntityQuery(typeof(CTargetableWall));

        protected override bool ShouldHaveIndicator(Entity candidate) =>
            Require(candidate, out CTakesDuration cDuration) && Require(candidate, out CDisplayDuration cDisplay) &&
            cDuration.Active && (cDisplay.ShowWhenEmpty || cDuration.Remaining < cDuration.Total);

        protected override Entity CreateIndicator(Entity source)
        {
            var indicator = base.CreateIndicator(source);
            Set<CProgressIndicator>(indicator);
            Set<CPosition>(indicator);
            UpdateIndicator(indicator, source);
            return indicator;
        }

        protected override void UpdateIndicator(Entity indicator, Entity source)
        {
            if (!Require(source, out CPosition pos) || !Require(source, out CTakesDuration cDuration) || !Require(source, out CDisplayDuration cDisplay))
                return;
            base.UpdateIndicator(indicator, source);

            float x = cDuration.Remaining / cDuration.Total;
            float progress = cDuration.IsInverse ? x : (1f - x);
            Set(indicator, new CProgressIndicator
            {
                IsBad = cDisplay.IsBad,
                Progress = progress,
                Process = cDisplay.Process,
                CurrentChange = cDuration.CurrentChange
            });
            Set<CPosition>(indicator, pos.Position);
        }

    }
}

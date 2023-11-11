using Kitchen;
using Kitchen.Layouts;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class InteractDestroyWall : ItemInteractionSystem
    {
        protected override bool RequirePress => false;
        protected override bool RequireHold => true;
        protected override InteractionType RequiredType => InteractionType.Act;

        private CToolUser cToolUser;
        private CDestructiveTool cDestTool;
        private CDestructive cDestructive;

        protected override bool IsPossible(ref InteractionData data) =>
            Require(data.Interactor, out cToolUser) && Require(cToolUser.CurrentTool, out cDestTool) && Require(cToolUser.CurrentTool, out cDestructive);

        protected override void Perform(ref InteractionData data)
        {
            data.ShouldAct = false;
            if (!Require(data.Interactor, out CPosition cPos))
                return;

            var tool = cToolUser.CurrentTool;

            if (Has<CTargetableWall>(cDestructive.Target))
            {
                data.ShouldAct = true;
                return;
            }

            var currentLocation = cPos.Position;
            var targetLocation = cPos.Position + cPos.Forward(1f);
            if (currentLocation.Rounded().IsDiagonal(targetLocation.Rounded()))
                return;

            var currentTile = GetTile(currentLocation);
            var targetTile = GetTile(targetLocation);
            if (!this.GetTargetableFeature(currentTile, targetTile, out var entity) || (Has<CReaching>(entity) && !cDestructive.DestroyToWall))
                return;

            var buffer = GetBuffer<CWallTargetedBy>(entity);
            buffer.Add(new()
            {
                Destroy = true,
                Hatch = !cDestructive.DestroyToWall,
                Interactor = tool
            });

            cDestructive.Target = entity;
            Set(tool, cDestructive);

            data.ShouldAct = true;
        }
    }
}

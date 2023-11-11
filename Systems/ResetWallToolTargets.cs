using Kitchen;
using KitchenRenovation.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    public class ResetWallToolTargets : GameSystemBase
    {
        EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CEquippableTool), typeof(CDestructive), typeof(CDestructiveTool), typeof(CToolInUse));
        }

        protected override void OnUpdate()
        {
            using var entities = Query.ToEntityArray(Allocator.Temp);
            using var toolsInUse = Query.ToComponentDataArray<CToolInUse>(Allocator.Temp);
            using var destructives = Query.ToComponentDataArray<CDestructive>(Allocator.Temp);
            for (int i = 0; i < toolsInUse.Length; i++)
            {
                var cDestructive = destructives[i];
                if (cDestructive.Target == Entity.Null || !Require(cDestructive.Target, out CTargetableWall cWall))
                    continue;

                var user = toolsInUse[i].User;
                if (!Require(user, out CAttemptingInteraction Attempt) || !Require(user, out CPosition cPos))
                    continue;

                if (cWall.Tile1.IsSameTile(cPos.Position) && cWall.Tile2.IsSameTile(cPos.Position + cPos.Forward(1f)) &&
                    Attempt.Type == InteractionType.Act)
                    continue;

                cDestructive.Target = Entity.Null;
                Set(entities[i], cDestructive);
            }
        }
    }
}

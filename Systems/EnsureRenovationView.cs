using Kitchen;
using KitchenRenovation.Components;
using KitchenRenovation.Utility;

namespace KitchenRenovation.Systems
{
    public class EnsureRenovationView : GameSystemBase
    {
        protected override void Initialise()
        {
            base.Initialise();
            GenericSystemBaseExt.Walls = GetEntityQuery(typeof(CTargetableWall), typeof(CPosition), typeof(CWallTargetedBy), typeof(CTakesDuration));
        }

        protected override void OnUpdate()
        {
            if (HasSingleton<SRenovation>())
                return;

            var singleton = EntityManager.CreateEntity(typeof(CDoNotPersist), typeof(CPersistThroughSceneChanges), typeof(SRenovation));
            Set(singleton, new CRequiresView
            {
                Type = RenovationView,
                ViewMode = ViewMode.World
            });
        }
    }
}

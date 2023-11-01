using Kitchen;
using KitchenRenovation.Components;
using UnityEngine;

namespace KitchenRenovation.Systems
{
    public class EnsureRenovationView : GameSystemBase
    {
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

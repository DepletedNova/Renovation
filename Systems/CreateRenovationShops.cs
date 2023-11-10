using Kitchen;
using KitchenData;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    public class CreateRenovationShops : StartOfNightSystem
    {
        protected override void OnUpdate()
        {
            if (HasSingleton<SIsRestartedDay>())
                return;

            var day = GetSingleton<SDay>().Day;
            if (day > 0 && day % 5 == 0)
            {
                AddShop(RenovationUtilityTag);
                AddShop(RenovationDestructiveTag);
            }
        }

        private void AddShop(ShoppingTags Tag)
        {
            Entity shopEntity = EntityManager.CreateEntity(typeof(CNewShop));
            Set(shopEntity, new CNewShop
            {
                Tags = Tag,
            });
        }

    }
}

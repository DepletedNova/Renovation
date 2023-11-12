using Kitchen.ShopBuilder;
using Unity.Entities;

namespace KitchenRenovation.Systems
{
    /*public class FilterByPreference : ShopBuilderFilter
    {
        protected override void Filter(ref CShopBuilderOption option)
        {
            if (option.IsRemoved)
                return;

            if (ShouldBlock(option.Appliance))
            {
                option.IsRemoved = true;
                option.FilteredBy = this;
            }
        }
    }*/
}

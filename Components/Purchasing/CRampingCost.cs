using KitchenData;

namespace KitchenRenovation.Components
{
    public struct CRampingCost : IApplianceProperty
    {
        public int IncreasedCost;
        public int DayIncrement;
        public int MinimumDay;
    }
}

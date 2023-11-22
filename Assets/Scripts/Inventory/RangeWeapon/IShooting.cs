using Infrastructure;

namespace Inventory
{
    public interface IShooting
    {
        public ObservableVariable<int> BulletsInMagazine { get; }
    }
}
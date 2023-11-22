using Infrastructure;

namespace Inventory
{
    public interface IReloading
    {
        public ObservableVariable<int> BulletsInMagazine { get; }
        public ObservableVariable<int> TotalBullets { get; }
    }
}
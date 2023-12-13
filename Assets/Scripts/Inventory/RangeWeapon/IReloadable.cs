using Infrastructure;

namespace Inventory.RangeWeapon
{
    public interface IReloading
    {
        public ObservableVariable<int> BulletsInMagazine { get; }
        public ObservableVariable<int> TotalBullets { get; }
    }
}
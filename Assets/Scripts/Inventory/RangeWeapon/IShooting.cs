using Infrastructure;

namespace Inventory.RangeWeapon
{
    public interface IShooting
    {
        public ObservableVariable<int> BulletsInMagazine { get; }
    }
}
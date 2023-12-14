using Infrastructure;

namespace Inventory.RocketLauncher
{
    public interface ILaunching
    {
        public ObservableVariable<int> RocketsInSlotsCount { get; }
    }
}
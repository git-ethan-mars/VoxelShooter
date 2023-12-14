using Infrastructure;

namespace Inventory.RocketLauncher
{
    public interface IRocketReloadable
    {
        public ObservableVariable<int> Count { get; }
        public ObservableVariable<int> RocketsInSlotsCount { get; }
    }
}
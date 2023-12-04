using Infrastructure;

namespace Inventory
{
    public interface IConsumable
    {
        public ObservableVariable<int> Count { get; set; }
    }
}
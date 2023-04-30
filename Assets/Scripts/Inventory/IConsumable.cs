namespace Inventory
{
    public interface IConsumable
    {
        public int Count { get; set; }
        void OnCountChanged();
    }
}
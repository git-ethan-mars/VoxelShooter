namespace Inventory
{
    public interface IInventoryItemState
    {
        IInventoryItemModel ItemModel { get; }
        IInventoryItemView ItemView { get; }
        void Enter();
        void Update();
        void Exit();
    }
}
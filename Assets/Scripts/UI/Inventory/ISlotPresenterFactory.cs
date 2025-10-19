using GamePlay.Core;
namespace UI.Inventory
{
	public interface ISlotPresenterFactory
	{
		SlotPresenter CreatePresenter(InventoryItem item, SlotView slotView);
	}
}
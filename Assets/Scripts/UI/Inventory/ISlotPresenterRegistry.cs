using System;
using GamePlay;
namespace UI.Inventory
{
	public interface ISlotPresenterRegistry
	{
		void Register<T>(Func<T, SlotView, SlotPresenter> creator) where T : InventoryItem;
		SlotPresenter Create(InventoryItem item, SlotView slotView);
	}
}
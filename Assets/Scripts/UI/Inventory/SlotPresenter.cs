using System;
using GamePlay;

namespace UI.Inventory
{
	public class SlotPresenter : IDisposable
	{
		private readonly SlotView _slotView;
		private readonly InventoryItem _inventoryItem;

		protected SlotPresenter(InventoryItem inventoryItem, SlotView slotView)
		{
			_inventoryItem = inventoryItem;
			_slotView = slotView;
		}

		public virtual void Initialize()
		{
			_inventoryItem.Selected += _slotView.Select;
			_inventoryItem.Deselected += _slotView.Deselect;
			
			_slotView.SetSlotIcon(_inventoryItem.InventoryIcon);
		}

		public virtual void Dispose()
		{
			_inventoryItem.Selected -= _slotView.Select;
			_inventoryItem.Deselected -= _slotView.Deselect;
		}
	}
}
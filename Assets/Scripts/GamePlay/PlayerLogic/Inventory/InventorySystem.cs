using System;
using System.Collections.Generic;

namespace GamePlay
{
	public class InventorySystem
	{
		public IReadOnlyList<InventoryItem> Items => _items;
		public int? ActiveSlotIndex { get; private set; }
		
		private readonly InventoryItem[] _items;
		private InventoryItem _selectedItem;

		public InventorySystem(List<InventoryItem> items)
		{
			_items = items.ToArray();
		}

		public void ChangeSlot(int slot)
		{
			if (slot < 0 || slot >= _items.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(slot), $"Slot number should be between 0 and {_items.Length}");
			}

			if (ActiveSlotIndex == slot)
			{
				return;
			}

			_selectedItem?.Deselect();
			_selectedItem = _items[slot];
			_selectedItem.Select();
			ActiveSlotIndex = slot;
		}

		public void Reset()
		{
			ActiveSlotIndex = null;
			_selectedItem?.Deselect();
			_selectedItem = null;
		}

		public void ApplyEffectToItems<T>(Action<T> effect) where T : InventoryItem
		{
			for (var i = 0; i < _items.Length; i++)
			{
				if (_items[i] as T)
				{
					effect((T)_items[i]);
				} 
			}
		}
	}
}
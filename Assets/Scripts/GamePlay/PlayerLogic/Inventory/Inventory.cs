using System;
using System.Collections.Generic;
using Common.StaticData;
using UnityEngine;

namespace Inventory
{
	public class Inventory : MonoBehaviour
	{
		private const int InventorySize = 6;

		private IInventoryItem[] _items = new IInventoryItem[InventorySize];
		private int _activeSlotId;

		public void Construct(List<IInventoryItem> items)
		{
			if (items.Count > InventorySize)
			{
				throw new ArgumentOutOfRangeException(nameof(items), "Amount of items is bigger than actual max inventory size");
			}

			_items = items.ToArray();
		}

		public void ChangeSlot(int slot)
		{
			if (slot < 0 || slot >= InventorySize)
			{
				throw new ArgumentOutOfRangeException(nameof(slot), $"Slot number should be between 0 and {InventorySize}");
			}

			if (slot == _activeSlotId)
			{
				return;
			}

			_items[_activeSlotId].Disable();
			_activeSlotId = slot;
			_items[_activeSlotId].Enable();
		}

		public void ApplyEffectToItems<T>(Action<T> effect) where T : IInventoryItem
		{
			for (var i = 0; i < _items.Length; i++)
			{
				if (_items[i] is T)
				{
					effect((T)_items[i]);
				} 
			}
		}
	}

	public interface IInventoryItem
	{
		void Enable();
		void Disable();
	}
}
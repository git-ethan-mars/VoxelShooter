using System;
using System.Collections.Generic;
using GamePlay.Core;
namespace UI.Inventory
{
	public class SlotPresenterRegistry : ISlotPresenterRegistry
	{
		private readonly Dictionary<Type, Func<InventoryItem, SlotView, SlotPresenter>> _creators = new();

		public void Register<T>(Func<T, SlotView, SlotPresenter> creator) where T : InventoryItem
		{
			_creators[typeof(T)] = (item, view) => creator((T)item, view);
		}

		// Убираем generic из возвращаемого типа!
		public SlotPresenter Create(InventoryItem item, SlotView slotView)
		{
			Type currentType = item.GetType();

			while (currentType != null && currentType != typeof(object))
			{
				if (_creators.TryGetValue(currentType, out var creator))
				{
					return creator(item, slotView);
				}
				currentType = currentType.BaseType;
			}

			throw new KeyNotFoundException($"No presenter registered for type hierarchy of: {item.GetType()}");
		}
	}
}
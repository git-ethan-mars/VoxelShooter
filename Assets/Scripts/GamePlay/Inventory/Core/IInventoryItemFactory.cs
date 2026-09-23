using System.Collections.Generic;
using Data;

namespace GamePlay
{
	public interface IItemFactory
	{
		List<InventoryItem> CreateItems(GameClass gameClass);
	}
}

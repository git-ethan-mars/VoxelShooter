using System.Collections.Generic;
using Data;
namespace GamePlay.Core
{
	public interface IItemFactory
	{
		List<InventoryItem> CreateItems(GameClass gameClass);
	}
}
using System.Collections.Generic;
using Data;
using GamePlay.Core;
using Mirror;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class ItemFactory : IItemFactory
	{
		private readonly IAssetProvider _assets;
		private readonly IStaticDataService _staticData;

		public ItemFactory(IAssetProvider assets, IStaticDataService staticData)
		{
			_assets = assets;
			_staticData = staticData;
		}

		public List<InventoryItem> CreateItems(GameClass gameClass)
		{
			var items = new List<InventoryItem>();
			
			foreach (ItemType itemType in _staticData.GetItems(gameClass))
			{
				GameObject itemPrefab = _staticData.GetItemPrefab(itemType);
				var item = _assets.Instantiate(itemPrefab).GetComponent<InventoryItem>();
				NetworkServer.Spawn(item.gameObject);
				items.Add(item);
			}

			return items;
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Infrastructure.Services
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<GameClass, GameInventory> _inventoryByClass;
        private Dictionary<int, InventoryItem> _itemById;

        public void LoadItems()
        {
            _itemById = Resources.LoadAll<InventoryItem>("StaticData/Inventory Items").ToDictionary(x => x.id, x => x);
        }

        public InventoryItem GetItem(int id)
        {
            return _itemById.TryGetValue(id, out var item) ? item : null;
        }

        public void LoadInventories()
        {
            _inventoryByClass = Resources.LoadAll<GameInventory>("StaticData/Inventories").ToDictionary(x=>x.gameClass, x => x);
        }

        public List<InventoryItem> GetInventory(GameClass gameClass)
        {
            return _inventoryByClass.TryGetValue(gameClass, out var gameInventory) ? gameInventory.inventory : null;
        }
    }
}
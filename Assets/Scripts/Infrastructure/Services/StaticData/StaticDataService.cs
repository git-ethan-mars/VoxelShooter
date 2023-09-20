﻿using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Infrastructure.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string Default = "Default";
        private Dictionary<GameClass, PlayerCharacteristic> _playerCharacteristicByClass;
        private Dictionary<GameClass, GameInventory> _inventoryByClass;
        private Dictionary<int, InventoryItem> _itemById;
        private Dictionary<string, MapConfigure> _mapConfigureByName;

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
            _inventoryByClass = Resources.LoadAll<GameInventory>("StaticData/Inventories")
                .ToDictionary(x => x.gameClass, x => x);
        }

        public List<InventoryItem> GetInventory(GameClass gameClass)
        {
            return _inventoryByClass.TryGetValue(gameClass, out var gameInventory) ? gameInventory.inventory : null;
        }

        public void LoadPlayerCharacteristics()
        {
            _playerCharacteristicByClass = Resources.LoadAll<PlayerCharacteristic>("StaticData/Player Characteristics")
                .ToDictionary(x => x.gameClass, x => x);
        }

        public PlayerCharacteristic GetPlayerCharacteristic(GameClass gameClass)
        {
            return _playerCharacteristicByClass.TryGetValue(gameClass, out var characteristic) ? characteristic : null;
        }

        public void LoadMapConfigures()
        {
            _mapConfigureByName = Resources.LoadAll<MapConfigure>("StaticData/Map configures")
                .ToDictionary(configure => configure.mapName, configure => configure);
        }

        public MapConfigure GetMapConfigure(string mapName)
        {
            return _mapConfigureByName.TryGetValue(mapName, out var mapConfigure)
                ? mapConfigure
                : GetDefaultMapConfigure();
        }

        public MapConfigure GetDefaultMapConfigure()
        {
            return _mapConfigureByName[Default];
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Infrastructure.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string Default = "Default";
        private const string InventoryItemsPath = "StaticData/Inventory Items";
        private const string InventoriesPath = "StaticData/Inventories";
        private const string PlayerCharacteristicsPath = "StaticData/Player Characteristics";
        private const string MapConfiguresPath = "StaticData/Map configures";
        private const string LobbyBalancePath = "StaticData/Lobby Balance";
        private const string BlockHealthBalancePath = "StaticData/Block Health";
        private Dictionary<GameClass, PlayerCharacteristic> _playerCharacteristicByClass;
        private Dictionary<GameClass, GameInventory> _inventoryByClass;
        private Dictionary<int, InventoryItem> _itemById;
        private Dictionary<string, MapConfigure> _mapConfigureByName;
        private LobbyBalance _lobbyBalance;
        private BlockHealthBalance _blockHealthBalance;

        public void LoadItems()
        {
            _itemById = Resources.LoadAll<InventoryItem>(InventoryItemsPath).ToDictionary(x => x.id, x => x);
        }

        public InventoryItem GetItem(int id)
        {
            return _itemById.TryGetValue(id, out var item) ? item : null;
        }

        public void LoadInventories()
        {
            _inventoryByClass = Resources.LoadAll<GameInventory>(InventoriesPath)
                .ToDictionary(x => x.gameClass, x => x);
        }

        public List<InventoryItem> GetInventory(GameClass gameClass)
        {
            return _inventoryByClass.TryGetValue(gameClass, out var gameInventory) ? gameInventory.inventory : null;
        }

        public void LoadPlayerCharacteristics()
        {
            _playerCharacteristicByClass = Resources.LoadAll<PlayerCharacteristic>(PlayerCharacteristicsPath)
                .ToDictionary(x => x.gameClass, x => x);
        }

        public PlayerCharacteristic GetPlayerCharacteristic(GameClass gameClass)
        {
            return _playerCharacteristicByClass.TryGetValue(gameClass, out var characteristic) ? characteristic : null;
        }

        public void LoadMapConfigures()
        {
            _mapConfigureByName = Resources.LoadAll<MapConfigure>(MapConfiguresPath)
                .ToDictionary(configure => configure.name, configure => configure);
        }

        public MapConfigure GetMapConfigure(string mapName)
        {
            return _mapConfigureByName.TryGetValue(mapName, out var mapConfigure)
                ? mapConfigure
                : GetDefaultMapConfigure();
        }

        public void LoadLobbyBalance()
        {
            _lobbyBalance = Resources.Load<LobbyBalance>(LobbyBalancePath);
        }

        public LobbyBalance GetLobbyBalance()
        {
            return _lobbyBalance;
        }

        public void LoadBlockHealthBalance()
        {
            _blockHealthBalance = Resources.Load<BlockHealthBalance>(BlockHealthBalancePath);
        }

        public BlockHealthBalance GetBlockHealthBalance()
        {
            return _blockHealthBalance;
        }

        private MapConfigure GetDefaultMapConfigure()
        {
            return _mapConfigureByName[Default];
        }
    }
}
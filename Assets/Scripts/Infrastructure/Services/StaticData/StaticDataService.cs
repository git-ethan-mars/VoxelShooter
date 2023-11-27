using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
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
        private const string SoundPath = "Audio/Sounds";
        private readonly IAssetProvider _assets;
        private Dictionary<GameClass, PlayerCharacteristic> _playerCharacteristicByClass;
        private Dictionary<GameClass, GameInventory> _inventoryByClass;
        private Dictionary<int, InventoryItem> _itemById;
        private Dictionary<string, MapConfigure> _mapConfigureByName;
        private Dictionary<int, AudioClip> _soundById;
        private LobbyBalance _lobbyBalance;

        public StaticDataService(IAssetProvider assets)
        {
            _assets = assets;
        }

        public void LoadItems()
        {
            _itemById = _assets.LoadAll<InventoryItem>(InventoryItemsPath).ToDictionary(x => x.id, x => x);
        }

        public InventoryItem GetItem(int id)
        {
            return _itemById.TryGetValue(id, out var item) ? item : null;
        }

        public void LoadInventories()
        {
            _inventoryByClass = _assets.LoadAll<GameInventory>(InventoriesPath)
                .ToDictionary(x => x.gameClass, x => x);
        }

        public List<InventoryItem> GetInventory(GameClass gameClass)
        {
            return _inventoryByClass.TryGetValue(gameClass, out var gameInventory) ? gameInventory.inventory : null;
        }

        public void LoadPlayerCharacteristics()
        {
            _playerCharacteristicByClass = _assets.LoadAll<PlayerCharacteristic>(PlayerCharacteristicsPath)
                .ToDictionary(x => x.gameClass, x => x);
        }

        public PlayerCharacteristic GetPlayerCharacteristic(GameClass gameClass)
        {
            return _playerCharacteristicByClass.TryGetValue(gameClass, out var characteristic) ? characteristic : null;
        }

        public void LoadMapConfigures()
        {
            _mapConfigureByName = _assets.LoadAll<MapConfigure>(MapConfiguresPath)
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
            _lobbyBalance = _assets.Load<LobbyBalance>(LobbyBalancePath);
        }

        public LobbyBalance GetLobbyBalance()
        {
            return _lobbyBalance;
        }

        private MapConfigure GetDefaultMapConfigure()
        {
            return _mapConfigureByName[Default];
        }


        public void LoadSounds()
        {
            _soundById = _assets.LoadAll<AudioClip>(SoundPath).Select((sound, index) => new
            {
                index, sound
            }).ToDictionary(it => it.index, it => it.sound);
        }

        public AudioClip GetSound(int soundId)
        {
            return _soundById.TryGetValue(soundId, out var sound) ? sound : null;
        }
    }
}
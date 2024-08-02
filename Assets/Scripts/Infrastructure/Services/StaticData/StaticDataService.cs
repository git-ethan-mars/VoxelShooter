using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;

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
        private const string SoundPath = "StaticData/Audio Data";
        private const string FallDamageConfigPath = "StaticData/Fall damage configuration";
        private const string BlueprintsPath = "StaticData/Blueprints";
        private readonly IAssetProvider _assets;
        private Dictionary<GameClass, PlayerCharacteristic> _playerCharacteristicByClass;
        private Dictionary<GameClass, GameInventory> _inventoryByClass;
        private Dictionary<int, InventoryItem> _itemById;
        private Dictionary<string, MapConfigure> _mapConfigureByName;
        private List<AudioData> _audios;
        private LobbyBalance _lobbyBalance;
        private BlockHealthBalance _blockHealthBalance;
        private FallDamageData _fallDamageData;

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

        public List<Blueprint> GetBlueprints()
        {
            return _assets.LoadAll<Blueprint>(BlueprintsPath).ToList();
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

        public void LoadBlockHealthBalance()
        {
            _blockHealthBalance = _assets.Load<BlockHealthBalance>(BlockHealthBalancePath);
        }

        public BlockHealthBalance GetBlockHealthBalance()
        {
            return _blockHealthBalance;
        }

        private MapConfigure GetDefaultMapConfigure()
        {
            return _mapConfigureByName[Default];
        }

        public void LoadSounds()
        {
            _audios = _assets.LoadAll<AudioData>(SoundPath).ToList();
        }

        public AudioData GetAudio(int soundId)
        {
            return _audios[soundId];
        }

        public int GetAudioIndex(AudioData audio)
        {
            var index = 0;
            for (var i = 0; i < _audios.Count; i++)
            {
                if (_audios[i] == audio)
                {
                    index = i;
                }
            }

            return index;
        }
        
        public void LoadFallDamageConfiguration()
        {
            _fallDamageData = _assets.Load<FallDamageData>(FallDamageConfigPath);
        }
        
        public FallDamageData GetFallDamageConfiguration()
        {
            return _fallDamageData;
        }
    }
}
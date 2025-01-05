using System;
using System.Collections.Generic;
using System.Linq;
using Common.AssetManagement;
using GamePlay.Data;

namespace GamePlay.Services
{
	public class StaticDataService : IStaticDataService
	{
		private const string InventoryItemsPath = "StaticData/Inventory Items";
		private const string InventoriesPath = "StaticData/Inventories";
		private const string PlayerCharacteristicsPath = "StaticData/Player Characteristics";
		private const string LobbyBalancePath = "StaticData/Lobby Balance";
		private const string VoxelHealthBalancePath = "StaticData/Block Health";
		private const string SoundPath = "StaticData/Audio Data";
		private const string FallDamageConfigPath = "StaticData/Fall damage configuration";
		private const string RectPaletteDataPath = "StaticData/Rect Palette Data";
		private readonly IAssetProvider _assets;
		private Dictionary<GameClass, PlayerCharacteristic> _playerCharacteristicByClass;
		private Dictionary<GameClass, GameInventory> _inventoryByClass;
		private Dictionary<int, ItemConfigure> _itemById;
		private List<AudioData> _audios;
		private LobbyBalance _lobbyBalance;
		private VoxelHealthBalance _voxelHealthBalance;
		private FallDamageData _fallDamageData;
		private RectPaletteData _rectPaletteData;

		public StaticDataService(IAssetProvider assets)
		{
			_assets = assets;
		}

		public void LoadItems()
		{
			_itemById = _assets.LoadAll<ItemConfigure>(InventoryItemsPath).ToDictionary(x => x.id, x => x);
		}

		public ItemConfigure GetItem(int id)
		{
			if (_itemById.TryGetValue(id, out var item))
			{
				return item;
			}

			throw new ArgumentOutOfRangeException(nameof(id));
		}

		public void LoadInventories()
		{
			_inventoryByClass = _assets.LoadAll<GameInventory>(InventoriesPath)
				.ToDictionary(x => x.gameClass, x => x);
		}

		public List<ItemConfigure> GetInventory(GameClass gameClass)
		{
			if (_inventoryByClass.TryGetValue(gameClass, out var inventory))
			{
				return inventory.items;
			}
			
			throw new ArgumentOutOfRangeException(nameof(gameClass));
		}

		public void LoadPlayerCharacteristics()
		{
			_playerCharacteristicByClass = _assets.LoadAll<PlayerCharacteristic>(PlayerCharacteristicsPath)
				.ToDictionary(x => x.gameClass, x => x);
		}

		public PlayerCharacteristic GetPlayerCharacteristic(GameClass gameClass)
		{
			if (_playerCharacteristicByClass.TryGetValue(gameClass, out var characteristic))
			{
				return characteristic;
			}

			throw new ArgumentOutOfRangeException(nameof(gameClass));
		}

		public void LoadLobbyBalance()
		{
			_lobbyBalance = _assets.Load<LobbyBalance>(LobbyBalancePath);
		}

		public LobbyBalance GetLobbyBalance()
		{
			return _lobbyBalance;
		}

		public void LoadVoxelHealthBalance()
		{
			_voxelHealthBalance = _assets.Load<VoxelHealthBalance>(VoxelHealthBalancePath);
		}

		public VoxelHealthBalance GetVoxelHealthBalance()
		{
			return _voxelHealthBalance;
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

		public void LoadRectPaletteData()
		{
			_rectPaletteData = _assets.Load<RectPaletteData>(RectPaletteDataPath);
		}

		public RectPaletteData GetRectPaletteData()
		{
			return _rectPaletteData;
		}
	}
}
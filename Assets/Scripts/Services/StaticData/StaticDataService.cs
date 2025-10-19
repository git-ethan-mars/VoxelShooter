using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
namespace Services
{
	public class StaticDataService : IStaticDataService
	{
		private const string ItemPrefabsPath = "StaticData/Item Prefabs";
		private const string InventoriesPath = "StaticData/Inventory Collection";
		private const string ItemConfigureCollectionPath = "StaticData/Item Configure Collection";
		private const string CrosshairSpritesPath = "StaticData/Crosshair Sprites Collection";
		private const string PlayerCharacteristicsPath = "StaticData/Characteristics Collection";
		private const string LobbyBalancePath = "StaticData/Lobby Balance";
		private const string VoxelHealthBalancePath = "StaticData/Block Health";
		private const string SoundPath = "StaticData/Audio Data";
		private const string RectPaletteDataPath = "StaticData/Rect Palette Data";
		private const string ItemIconsPath = "StaticData/Item Icons";
		private readonly IAssetProvider _assets;
		private List<AudioData> _audios;
		private IReadOnlyList<Sprite> _crosshairSprites;
		private Dictionary<ItemType, InventoryItemConfigure> _itemConfigures;
		private Dictionary<ItemType, Sprite> _slotIconByItemType;
		private Dictionary<ItemType, Sprite> _projectileIconByItemType;
		private Dictionary<ItemType, Sprite> _scopeIconByItemType;
		private Dictionary<GameClass, List<ItemType>> _inventoryByGameClass;
		private Dictionary<GameClass, Characteristics> _characteristicByGameClass;
		private Dictionary<ItemType, GameObject> _itemPrefabByItemType;
		private LobbyBalance _lobbyBalance;
		private RectPaletteData _rectPaletteData;
		private VoxelHealthBalance _voxelHealthBalance;

		public StaticDataService(IAssetProvider assets)
		{
			_assets = assets;
		}

		public void Initialize()
		{
			LoadItems();
			LoadItemIcons();
			LoadCharacteristics();
			LoadCrosshairSprites();
			LoadLobbyBalance();
			LoadVoxelHealthBalance();
			LoadSounds();
			LoadRectPaletteData();
		}

		private void LoadItemIcons()
		{
			var itemIcons = _assets.Load<ItemIconCollection>(ItemIconsPath);
			_slotIconByItemType = itemIcons.SlotIconByItemType;
			_projectileIconByItemType = itemIcons.ProjectileIconByItemType;
			_scopeIconByItemType = itemIcons.ScopeIconByItemType;
		}

		public IReadOnlyList<ItemType> GetItems(GameClass gameClass)
		{
			if (_inventoryByGameClass.TryGetValue(gameClass, out var items))
			{
				return items;
			}

			throw new ArgumentOutOfRangeException(nameof(gameClass));
		}

		public GameObject GetItemPrefab(ItemType itemType)
		{
			if (_itemPrefabByItemType.TryGetValue(itemType, out GameObject itemPrefab))
			{
				return itemPrefab;
			}

			throw new ArgumentOutOfRangeException(nameof(itemType));
		}

		public T GetItemConfigure<T>(ItemType itemType) where T : InventoryItemConfigure
		{
			if (_itemConfigures.TryGetValue(itemType, out InventoryItemConfigure itemConfigure))
			{
				return (T)itemConfigure;
			}

			throw new ArgumentOutOfRangeException(nameof(itemType));
		}

		public Characteristics GetCharacteristics(GameClass gameClass)
		{
			if (_characteristicByGameClass.TryGetValue(gameClass, out Characteristics characteristic))
			{
				return characteristic;
			}

			throw new ArgumentOutOfRangeException(nameof(gameClass));
		}

		public LobbyBalance GetLobbyBalance()
		{
			return _lobbyBalance;
		}

		public VoxelHealthBalance GetVoxelHealthBalance()
		{
			return _voxelHealthBalance;
		}

		public AudioData GetAudio(int soundId)
		{
			return _audios[soundId];
		}

		public RectPaletteData GetRectPaletteData()
		{
			return _rectPaletteData;
		}

		public Sprite GetSlotIcon(ItemType type)
		{
			return _slotIconByItemType[type];
		}

		public Sprite GetProjectileIcon(ItemType type)
		{
			return _projectileIconByItemType[type];
		}

		public Sprite GetScopeIcon(ItemType type)
		{
			return _scopeIconByItemType[type];
		}

		public CrosshairSprite GetCrosshairSprite(int id)
		{
			Sprite sprite = _crosshairSprites[id];
			var crosshairSprite = new CrosshairSprite(id, sprite);
			return crosshairSprite;
		}

		public IEnumerable<CrosshairSprite> GetCrosshairSprites()
		{
			return _crosshairSprites.Select((sprite, id) => new CrosshairSprite(id, sprite));
		}

		private void LoadItems()
		{
			_itemPrefabByItemType = _assets.Load<ItemPrefabCollection>(ItemPrefabsPath).ItemPrefabByItemType;
			_itemConfigures = _assets.Load<ItemConfigureCollection>(ItemConfigureCollectionPath).ConfigureByItemType;
			_inventoryByGameClass = _assets.Load<InventoryCollection>(InventoriesPath).Inventory;
		}

		private void LoadCharacteristics()
		{
			_characteristicByGameClass = _assets.Load<CharacteristicsCollection>(PlayerCharacteristicsPath).CharacteristicByItemType;
		}

		private void LoadLobbyBalance()
		{
			_lobbyBalance = _assets.Load<LobbyBalance>(LobbyBalancePath);
		}

		private void LoadVoxelHealthBalance()
		{
			_voxelHealthBalance = _assets.Load<VoxelHealthBalance>(VoxelHealthBalancePath);
		}

		private void LoadSounds()
		{
			_audios = _assets.LoadAll<AudioData>(SoundPath).ToList();
		}

		private void LoadRectPaletteData()
		{
			_rectPaletteData = _assets.Load<RectPaletteData>(RectPaletteDataPath);
		}

		private void LoadCrosshairSprites()
		{
			_crosshairSprites = _assets.Load<CrosshairSpriteCollection>(CrosshairSpritesPath).Sprites;
		}
	}
}
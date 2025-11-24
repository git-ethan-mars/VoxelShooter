using System.Collections.Generic;
using Data;
using UnityEngine;
using AudioType = Data.AudioType;
namespace Services
{
	public interface IStaticDataService
	{
		void Initialize();
		TConfigure GetItemConfigure<TConfigure>(ItemType itemType) where TConfigure : InventoryItemConfigure;
		IReadOnlyList<ItemType> GetItems(GameClass gameClass);
		GameObject GetItemPrefab(ItemType itemType);
		Characteristics GetCharacteristics(GameClass gameClass);
		LobbyBalance GetLobbyBalance();
		AudioData GetAudioData(AudioType audioType);
		RectPaletteData GetRectPaletteData();
		Sprite GetSlotIcon(ItemType type);
		Sprite GetProjectileIcon(ItemType type);
		Sprite GetScopeIcon(ItemType type);
		IEnumerable<CrosshairSprite> GetCrosshairSprites();
		CrosshairSprite GetCrosshairSprite(int id);
	}
}
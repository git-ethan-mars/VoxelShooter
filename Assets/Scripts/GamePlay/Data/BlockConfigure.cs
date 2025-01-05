using Common;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
	[CreateAssetMenu(fileName = "Block", menuName = "Inventory System/Inventory Items/Block")]
	public class BlockConfigure : ItemConfigure
	{
		private const string BlockPath = "Prefabs/Block";

		public Sprite itemSprite;
		[Header("Configuration")] public int count;

		public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
		{
			return inventoryFactory.CreateBlock(new BlockData(this), rayCaster);
		}
	}
}
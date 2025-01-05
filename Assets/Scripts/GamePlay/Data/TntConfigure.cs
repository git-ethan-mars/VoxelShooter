using Common;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
	[CreateAssetMenu(fileName = "Tnt", menuName = "Inventory System/Inventory Items/Tnt")]
	public class TntConfigure : ItemConfigure
	{
		public Sprite countIcon;
		public int count;
		public float delayInSeconds;
		public int radius;
		public int damage;
		public int particlesSpeed;
		public int particlesCount;
		public AudioData explosionSound;
		public AudioData countdownSound;

		public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
		{
			return inventoryFactory.CreateTnt(new TntData(this), rayCaster);
		}
	}
}
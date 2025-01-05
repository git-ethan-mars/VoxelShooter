using Common;
using Common.AssetManagement;
using GamePlay.Entities;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
	[CreateAssetMenu(fileName = "Grenade", menuName = "Inventory System/Inventory Items/Grenade")]
	public class GrenadeConfigure : ItemConfigure
	{
		public Sprite countIcon;
		public int count;
		public float delayInSeconds;
		public int radius;
		public int damage;
		public float maxThrowDuration;
		public float throwForceModifier;
		public float minThrowForce;
		public int particlesSpeed;
		public int particlesCount;
		public AudioData explosionSound;

		public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
		{
			return inventoryFactory.CreateGrenade(new GrenadeData(this), rayCaster);
		}
	}
}
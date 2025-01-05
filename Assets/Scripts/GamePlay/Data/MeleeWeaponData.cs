using UnityEngine;

namespace GamePlay.Data
{
	public class MeleeWeaponData : IItemData
	{
		public int ID => _meleeWeapon.id;
		public Sprite InventoryIcon => _meleeWeapon.inventoryIcon;
		public MeleeWeaponType Type => _meleeWeapon.type;
		public float Range => _meleeWeapon.range;
		public float TimeBetweenHit => _meleeWeapon.timeBetweenHit;
		public float HeadMultiplier => _meleeWeapon.headMultiplier;
		public float LegMultiplier => _meleeWeapon.legMultiplier;
		public float ChestMultiplier => _meleeWeapon.chestMultiplier;
		public float ArmMultiplier => _meleeWeapon.armMultiplier;
		public int DamageToPlayer => _meleeWeapon.damageToPlayer;
		public int DamageToBlock => _meleeWeapon.damageToVoxel;
		public bool HasStrongHit => _meleeWeapon.hasStrongHit;
		public AudioData DiggingAudio => _meleeWeapon.diggingAudio;
		public AudioData HittingAudio => _meleeWeapon.hittingAudio;
		public bool IsReady { get; set; } = true;

		private readonly MeleeWeaponConfigure _meleeWeapon;

		public MeleeWeaponData(MeleeWeaponConfigure meleeWeapon)
		{
			_meleeWeapon = meleeWeapon;
		}
	}
}
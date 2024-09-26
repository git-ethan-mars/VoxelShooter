namespace Common.StaticData
{
	public class MeleeWeaponData : IItemData
	{
		public int ID => _meleeWeapon.id;
		public float Range => _meleeWeapon.range;
		public float TimeBetweenHit => _meleeWeapon.timeBetweenHit;
		public float HeadMultiplier => _meleeWeapon.headMultiplier;
		public float LegMultiplier => _meleeWeapon.legMultiplier;
		public float ChestMultiplier => _meleeWeapon.chestMultiplier;
		public float ArmMultiplier => _meleeWeapon.armMultiplier;
		public int DamageToPlayer => _meleeWeapon.damageToPlayer;
		public int DamageToBlock => _meleeWeapon.damageToBlock;
		public bool HasStrongHit => _meleeWeapon.hasStrongHit;
		public AudioData DiggingAudio => _meleeWeapon.diggingAudio;
		public AudioData HittingAudio => _meleeWeapon.hittingAudio;
		public bool IsReady { get; set; } = true;

		private readonly MeleeWeaponItem _meleeWeapon;

		public MeleeWeaponData(MeleeWeaponItem meleeWeapon)
		{
			_meleeWeapon = meleeWeapon;
		}
	}
}
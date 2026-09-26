using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "Melee Weapon Configure", menuName = "Inventory System/Item Configures/Melee Weapon Configure")]
	public class MeleeWeaponConfigure : InventoryItemConfigure
	{
		[field: Header("Configure")]
		[field: SerializeField]
		public float AttackRange { get; private set; }

		[field: SerializeField] public float DiggingRange { get; private set; }

		[field: SerializeField] public float TimeBetweenHit { get; private set; }
		[field: SerializeField] public int DamageToPlayer { get; private set; }
		[field: SerializeField] public float HeadMultiplier { get; private set; }
		[field: SerializeField] public float ChestMultiplier { get; private set; }
		[field: SerializeField] public float LegMultiplier { get; private set; }
		[field: SerializeField] public float ArmMultiplier { get; private set; }
		[field: SerializeField] public int DamageToVoxel { get; private set; }
		[field: SerializeField] public bool HasStrongHit { get; private set; }
	}
}

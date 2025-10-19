using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Range Weapon", menuName = "Inventory System/Inventory Items/Range Weapon")]
	public class RangeWeaponConfigure : InventoryItemConfigure
	{
		[field: Header("Configuration")]
		[field: SerializeField] public bool IsAutomatic { get; private set; }
		[field: SerializeField] public float TimeBetweenShooting { get; private set; }
		[field: SerializeField] public float BaseRecoil { get; private set; }
		[field: SerializeField] public float StepRecoil { get; private set; }
		[field: SerializeField] public float ResetTimeRecoil { get; private set; }
		[field: SerializeField] public float Range { get; private set; }
		[field: SerializeField] public float ReloadTime { get; private set; }
		[field: SerializeField] public int MagazineSize { get; private set; }
		[field: SerializeField] public int BulletsPerTap { get; private set; }
		[field: SerializeField] public int TotalBullets { get; private set; }
		[field: SerializeField] public float ZoomMultiplier { get; private set; }
		[field: Header("Damage")]
		[field: SerializeField] public int Damage { get; private set; }
		[field: SerializeField] public float HeadMultiplier { get; private set; }
		[field: SerializeField] public float ChestMultiplier { get; private set; }
		[field: SerializeField] public float LegMultiplier { get; private set; }
		[field: SerializeField] public float ArmMultiplier { get; private set; }
	}
}
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Drill Launcher Configure", menuName = "Inventory System/Item Configures/Drill Launcher Configure")]
	public class DrillLauncherConfigure : InventoryItemConfigure
	{
		[field: Header("Configuration")]
		[field: SerializeField] public int Amount { get; private set; }
		[field: SerializeField] public ExplosionData ExplosionData { get; private set; }
		[field: SerializeField] public int Speed { get; private set; }
		[field: SerializeField] public int LifeTime { get; private set; }
		[field: SerializeField] public int RotationSpeed { get; private set; }
		[field: SerializeField] public float ReloadTime { get; private set; }
	}
}
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "TNT Configure", menuName = "Inventory System/Item Configures/TNT Configure")]
	public class TNTConfigure : InventoryItemConfigure
	{
		[field: Header("Configuration")]
		[field: SerializeField] public int Amount { get; private set; }
		[field: SerializeField] public int DelayInSeconds { get; private set; }
		[field: SerializeField] public ExplosionData ExplosionData { get; private set; }
		[field: SerializeField] public int ParticleSpeed { get; private set; }
		[field: SerializeField] public int ParticleCount { get; private set; }
	}
}
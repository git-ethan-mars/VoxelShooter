using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Rocket Launcher Configure", menuName = "Inventory System/Item Configures/Rocket Launcher Configure")]
	public class RocketLauncherConfigure : InventoryItemConfigure
	{
		[field: Header("Configure")]
		[field: SerializeField] public int Amount { get; private set; }
		[field: SerializeField] public ExplosionData ExplosionData { get; private set; }
		[field: SerializeField] public int Speed { get; private set; }
		[field: SerializeField] public float ReloadTime { get; private set; }
		[field: SerializeField] public int ChargedRocketsCapacity { get; private set; }
		[field: SerializeField] public int ParticleSpeed { get; private set; }
		[field: SerializeField] public int ParticleCount { get; private set; }
		[field: Header("Sound")]
		[field: SerializeField] public AudioData ExplosionSound { get; private set; }
		[field: SerializeField] public AudioData ReloadSound { get; private set; }
	}
}
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Grenade Configure", menuName = "Inventory System/Item Configures/Grenade Configure")]
	public class GrenadeConfigure : InventoryItemConfigure
	{
		[field: Header("Configuration")]
		[field: SerializeField] public int Amount { get; private set; }
		[field: SerializeField] public float DelayInSeconds { get; private set; }
		[field: SerializeField] public int Radius { get; private set; }
		[field: SerializeField] public int Damage { get; private set; }
		[field: SerializeField] public float MaxThrowDuration { get; private set; }
		[field: SerializeField] public float ThrowForceModifier { get; private set; }
		[field: SerializeField] public float MinThrowForce { get; private set; }
		[field: SerializeField] public float ParticleSpeed { get; private set; }
		[field: SerializeField] public float ParticleCount { get; private set; }
		[field: Header("Sound")]
		[field: SerializeField] public AudioData ExplosionSound { get; private set; }
	}
}
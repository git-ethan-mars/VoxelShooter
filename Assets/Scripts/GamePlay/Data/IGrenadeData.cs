using UnityEngine;

namespace GamePlay.Data
{
	public interface IGrenadeData : IItemData
	{
		int Amount { get; set; }
		Sprite CountIcon { get; }
		int Count { get; }
		float DelayInSeconds { get; }
		int Radius { get; }
		int Damage { get; }
		float MaxThrowDuration { get; }
		float ThrowForceModifier { get; }
		float MinThrowForce { get; }
		int ParticlesSpeed { get; }
		int ParticlesCount { get; }
		AudioData ExplosionSound { get; }
	}
}
using UnityEngine;

namespace Common.StaticData
{
	public class GrenadeData : IItemData
	{
		public int ID => _grenade.id;
		public int Amount { get; set; }
		public Sprite CountIcon => _grenade.countIcon;
		public int Count => _grenade.count;
		public float DelayInSeconds => _grenade.delayInSeconds;
		public int Radius => _grenade.radius;
		public int Damage => _grenade.damage;
		public float MaxThrowDuration => _grenade.maxThrowDuration;
		public float ThrowForceModifier => _grenade.throwForceModifier;
		public float MinThrowForce => _grenade.minThrowForce;
		public int ParticlesSpeed => _grenade.particlesSpeed;
		public int ParticlesCount => _grenade.particlesCount;
		public AudioData ExplosionSound => _grenade.explosionSound;
		private readonly GrenadeItem _grenade;

		public GrenadeData(GrenadeItem grenade)
		{
			_grenade = grenade;
			Amount = grenade.count;
		}
	}
}
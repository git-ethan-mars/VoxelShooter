using System;
using UnityEngine;

namespace GamePlay.Data
{
	public class GrenadeData : IGrenadeData
	{
		public int ID => _grenade.id;
		public Sprite InventoryIcon => _grenade.inventoryIcon;
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

		public int Amount
		{
			get => _amount;
			set
			{
				_amount = value;
				AmountChanged?.Invoke(value);
			}
		}

		private int _amount;
		public event Action<int> AmountChanged;

		private readonly GrenadeConfigure _grenade;

		public GrenadeData(GrenadeConfigure grenade)
		{
			_grenade = grenade;
			_amount = grenade.count;
		}
	}
}
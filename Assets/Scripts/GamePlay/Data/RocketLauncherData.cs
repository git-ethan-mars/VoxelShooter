using System;
using UnityEngine;

namespace GamePlay.Data
{
	public class RocketLauncherData : IRocketLauncherData
	{
		public int ID => _rocketLauncher.id;
		public Sprite InventoryIcon => _rocketLauncher.inventoryIcon;
		public float Speed => _rocketLauncher.speed;
		public int ChargedRocketsCapacity => _rocketLauncher.chargedRocketsCapacity;
		public int RechargeableRocketsCount => _rocketLauncher.rechargeableRocketsCount;
		public float ReloadTime => _rocketLauncher.reloadTime;
		public AudioData ReloadSound => _rocketLauncher.reloadSound;
		public int ParticlesSpeed => _rocketLauncher.particlesSpeed;
		public int ParticlesCount => _rocketLauncher.particlesCount;
		public AudioData ExplosionSound => _rocketLauncher.explosionSound;
		public bool IsReloading { get; set; }

		public int CarriedRockets
		{
			get => _carriedRockets;
			set
			{
				_carriedRockets = value;
				CarriedRocketsChanged?.Invoke(value);
			}
		}

		public event Action<int> CarriedRocketsChanged;

		private int _carriedRockets;


		public int ChargedRockets
		{
			get => _chargedRockets;
			set
			{
				_chargedRockets = value;
				ChargedRocketsChanged?.Invoke(value);
			}
		}

		public event Action<int> ChargedRocketsChanged;

		private int _chargedRockets;

		private readonly RocketLauncherConfigure _rocketLauncher;


		public RocketLauncherData(RocketLauncherConfigure rocketLauncher)
		{
			_rocketLauncher = rocketLauncher;
			_carriedRockets = rocketLauncher.count;
			_chargedRockets = rocketLauncher.chargedRocketsCapacity;
			IsReloading = false;
		}
	}
}
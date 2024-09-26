namespace Common.StaticData
{
	public class RocketLauncherData : IItemData
	{
		public int ID => _rocketLauncher.id;
		public float Speed => _rocketLauncher.speed;
		public int ChargedRocketsCapacity => _rocketLauncher.chargedRocketsCapacity;
		public int RechargeableRocketsCount => _rocketLauncher.rechargeableRocketsCount;
		public float ReloadTime => _rocketLauncher.reloadTime;
		public AudioData ReloadSound => _rocketLauncher.reloadSound;
		public int ParticlesSpeed => _rocketLauncher.particlesSpeed;
		public int ParticlesCount => _rocketLauncher.particlesCount;
		public AudioData ExplosionSound => _rocketLauncher.explosionSound;
		public int CarriedRockets { get; set; }
		public int ChargedRockets { get; set; }
		public bool IsReloading { get; set; }

		private readonly RocketLauncherItem _rocketLauncher;

		public RocketLauncherData(RocketLauncherItem rocketLauncher)
		{
			_rocketLauncher = rocketLauncher;
			CarriedRockets = rocketLauncher.count;
			ChargedRockets = rocketLauncher.chargedRocketsCapacity;
			IsReloading = false;
		}
	}
}
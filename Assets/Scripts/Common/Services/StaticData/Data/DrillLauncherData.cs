namespace Common.StaticData
{
	public class DrillLauncherData : IItemData
	{
		public int ID => _drillLauncher.id;
		public int Speed => _drillLauncher.speed;
		public int RotationSpeed => _drillLauncher.rotationSpeed;
		public float Lifetime => _drillLauncher.lifetime;
		public float ReloadTime => _drillLauncher.reloadTime;
		public AudioData ReloadSound => _drillLauncher.reloadSound;
		public int ChargedDrills { get; set; }
		public bool IsReloading { get; set; }
		public int Amount { get; set; }

		private readonly DrillLauncherItem _drillLauncher;

		public DrillLauncherData(DrillLauncherItem drillLauncher)
		{
			_drillLauncher = drillLauncher;
			Amount = drillLauncher.count;
			ChargedDrills = drillLauncher.chargedDrillsCapacity;
		}
	}
}
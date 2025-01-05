namespace GamePlay.Data
{
	public interface IRocketLauncherData : IItemData
	{
		float Speed { get; }
		int ChargedRocketsCapacity { get; }
		int RechargeableRocketsCount { get; }
		float ReloadTime { get; }
		AudioData ReloadSound { get; }
		int ParticlesSpeed { get; }
		int ParticlesCount { get; }
		AudioData ExplosionSound { get; }
		int CarriedRockets { get; set; }
		int ChargedRockets { get; set; }
		bool IsReloading { get; set; }
	}
}
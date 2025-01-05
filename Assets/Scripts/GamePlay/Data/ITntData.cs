namespace GamePlay.Data
{
	public interface ITntData : IItemData
	{
		float DelayInSeconds { get; }
		AudioData CountdownSound { get; }
		int ParticlesSpeed { get; }
		int ParticlesCount { get; }
		AudioData ExplosionSound { get; }
		int Amount { get; set; }
	}
}
namespace Common.StaticData
{
	public class TntData : IItemData
	{
		public int ID => _tnt.id;
		public float DelayInSeconds => _tnt.delayInSeconds;
		public AudioData CountdownSound => _tnt.countdownSound;
		public int ParticlesSpeed => _tnt.particlesSpeed;
		public int ParticlesCount => _tnt.particlesCount;
		public AudioData ExplosionSound => _tnt.explosionSound;
		public int Amount { get; set; }

		private readonly TntItem _tnt;

		public TntData(TntItem tnt)
		{
			_tnt = tnt;
			Amount = tnt.count;
		}
	}
}
using UnityEngine;

namespace GamePlay.Data
{
	public class TntData : ITntData 
	{
		public int ID => _tnt.id;
		public Sprite InventoryIcon => _tnt.inventoryIcon;
		public float DelayInSeconds => _tnt.delayInSeconds;
		public AudioData CountdownSound => _tnt.countdownSound;
		public int ParticlesSpeed => _tnt.particlesSpeed;
		public int ParticlesCount => _tnt.particlesCount;
		public AudioData ExplosionSound => _tnt.explosionSound;
		public int Amount { get; set; }

		private readonly TntConfigure _tnt;

		public TntData(TntConfigure tnt)
		{
			_tnt = tnt;
			Amount = tnt.count;
		}
	}
}
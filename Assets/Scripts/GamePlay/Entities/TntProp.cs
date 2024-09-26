using System;
using Common.Audio;
using Common.Factory;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using GamePlay.Destruction;
using UnityEngine;

namespace GamePlay.Entities
{
	public class TntProp : Entity
	{
		[SerializeField]
		private Explosion explosion;
		
		private IParticleFactory _particleFactory;
		private IAudioPlayer _audioPlayer;
		private TntData _data;

		public void Construct(IParticleFactory particleFactory, IAudioPlayer audioPlayer, TntData data)
		{
			_particleFactory = particleFactory;
			_audioPlayer = audioPlayer;
			_data = data;
		}

		public async UniTask ExplodeAsync()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(_data.DelayInSeconds));
			explosion.Explode();
		}

		protected void OnExploded()
		{
			
		}
	}
}
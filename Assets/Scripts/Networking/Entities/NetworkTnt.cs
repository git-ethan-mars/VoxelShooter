using Common.Factory;
using Common.StaticData;
using Explosions;
using GamePlay.Entities;
using Infrastructure.Factory;
using Infrastructure.Services.Audio;
using Mirror;
using UnityEngine;

namespace Entities
{
	public class NetworkTnt : Entity
	{
		public Vector3 Position => transform.position;
		private IParticleFactory _particleFactory;
		private TntData _tntData;
		private ExplosionBehaviour _explosionBehaviour;
		private IAudioPlayer _audioPlayer;

		public void Construct(IParticleFactory particleFactory, IAudioPlayer audioPlayer, TntData tntData)
		{
			_particleFactory = particleFactory;
			_audioPlayer = audioPlayer;
			_tntData = tntData;
			
		}

		public override void OnStartClient()
		{
			_audioPlayer.Play(transform.position, _tntData.CountdownSound);
		}

		[ClientRpc]
		public void RpcExplode()
		{
			_particleFactory.CreateRchParticle(transform.position, _tntData.ParticlesSpeed,
				_tntData.ParticlesCount);
			_audioPlayer.Play(transform.position, _tntData.ExplosionSound);
			Destroy(gameObject);
		}
	}
}
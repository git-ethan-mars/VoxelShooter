using Common.Audio;
using Common.Factory;
using Common.StaticData;
using GamePlay.Entities;
using Mirror;
using UnityEngine;

namespace Networking.Entities
{
	[RequireComponent(typeof(Grenade))]
	public class NetworkGrenade : NetworkEntity
	{
		[SerializeField]
		private Grenade grenade;

		private IParticleFactory _particleFactory;
		private IAudioPlayer _audioPlayer;
		private GrenadeData _data;

		public void Construct(IParticleFactory particleFactory, IAudioPlayer audioPlayer, GrenadeData data)
		{
			grenade.Construct(particleFactory, audioPlayer, data);
			_particleFactory = particleFactory;
			_audioPlayer = audioPlayer;
			_data = data;
		}

		[Server]
		public void Explode()
		{
			grenade.Explode();
			OnExploded();
		}
		
		[ClientRpc]
		private void OnExploded()
		{
			_particleFactory.CreateRchParticle(transform.position, _data.ParticlesSpeed,
				_data.ParticlesCount);
			_audioPlayer.PlayAsync(transform.position, _data.ExplosionSound);
		}
	}
}
using Infrastructure.Factory;
using Mirror;
using System;
using Common.Factory;
using Common.StaticData;
using GamePlay.Entities;
using Infrastructure.Services.Audio;
using UnityEngine;

namespace Entities
{
    public class Rocket : Entity
    {
        public event Action Collided;

        private IParticleFactory _particleFactory;
        private IAudioPlayer _audioPlayer;
        private RocketLauncherData _rocketData;

        public void Construct(IParticleFactory particleFactory, IAudioPlayer audioPlayer, RocketLauncherData rocketData)
        {
            _particleFactory = particleFactory;
            _audioPlayer = audioPlayer;
            _rocketData = rocketData;
        }

        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponentInParent<NetworkIdentity>()?.connectionToClient == connectionToClient)
            {
                return;
            }

            Collided?.Invoke();
        }

        [ClientRpc]
        public void RpcExplode()
        {
            _particleFactory.CreateRchParticle(transform.position, _rocketData.ParticlesSpeed,
                _rocketData.ParticlesCount);
            _audioPlayer.Play(transform.position, _rocketData.ExplosionSound);
        }
    }
}
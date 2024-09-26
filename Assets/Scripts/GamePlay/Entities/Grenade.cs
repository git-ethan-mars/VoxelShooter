using Common.Audio;
using Common.Factory;
using Common.StaticData;
using Entities;
using GamePlay.Destruction;
using Inventory;
using UnityEngine;

namespace GamePlay.Entities
{
    public class Grenade : Entity, IInventoryItem
    {
        [SerializeField]
        private Explosion explosion;
        private IParticleFactory _particleFactory;
        public GrenadeData Data;
        private IAudioPlayer _audioPlayer;

        public void Construct(IParticleFactory particleFactory, IAudioPlayer audioPlayer, GrenadeData grenadeData)
        {
            _particleFactory = particleFactory;
            _audioPlayer = audioPlayer;
            Data = grenadeData;
        }

        public void Enable()
        {
            throw new System.NotImplementedException();
        }
        
        public void Explode()
        {
            explosion.Explode();
        }

        public void Disable()
        {
            throw new System.NotImplementedException();
        }
    }
}
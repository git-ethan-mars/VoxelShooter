using System.Linq;
using GamePlay.Data;
using GamePlay.Factory;
using GamePlay.MapFeatures;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Entities
{
    public class Rocket : Entity
    {
        [SerializeField]
        private ExplosionData explosionData;
        
        private IParticleFactory _particleFactory;
        private RocketLauncherData _rocketData;
        private MapProvider _mapProvider;

        public void Construct(MapProvider mapProvider, IParticleFactory particleFactory, RocketLauncherData rocketData)
        {
            _mapProvider = mapProvider;
            _particleFactory = particleFactory;
            _rocketData = rocketData;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_mapProvider.TryGetMapFeature<MapDestruction>(out var mapDestruction))
            {
                mapDestruction.Visit(transform.position, explosionData);
            }
            
            foreach (var visitor in GetEntitiesByType<IDamageVisitor>())
            {
                visitor.Visit(transform.position, explosionData);
            }
        }
    }
}
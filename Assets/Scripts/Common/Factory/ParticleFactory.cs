using System.Collections;
using Common.AssetManagement;
using Common.StaticData;
using UnityEngine;

namespace Common.Factory
{
    public class ParticleFactory : IParticleFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private readonly ICoroutineRunner _coroutineRunner;

        public ParticleFactory(IAssetProvider assets, IStaticDataService staticData, ICoroutineRunner coroutineRunner)
        {
            _assets = assets;
            _staticData = staticData;
            _coroutineRunner = coroutineRunner;
        }

        public ParticleSystem CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 color)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletImpactPath, position, rotation).GetComponent<ParticleSystem>();
            var particleColor = bullet.GetComponent<ParticleColor>();
            particleColor.color = color;
            _coroutineRunner.StartCoroutine(DestroyParticle(bullet.gameObject,
                bullet.GetComponent<ParticleSystem>().main.startLifetime.constant));
            return bullet;
        }
        
        public ParticleSystem CreateBlockDestructionParticle(Vector3 position, Quaternion rotation, Color32 blockColor)
        {
            var particle = _assets.Instantiate(ParticlePath.BlockDestructionParticlePath, position, rotation).GetComponent<ParticleSystem>();
            var particleColor = particle.GetComponent<ParticleColor>();
            particleColor.color = blockColor;
            _coroutineRunner.StartCoroutine(DestroyParticle(particle.gameObject,
                particle.GetComponent<ParticleSystem>().main.startLifetime.constant));
            return particle;
        }

        public ParticleSystem CreateBlood(Vector3 position, Quaternion rotation)
        {
            var blood = _assets.Instantiate(ParticlePath.BloodSprayPath, position, rotation).GetComponent<ParticleSystem>();
            _coroutineRunner.StartCoroutine(DestroyParticle(blood.gameObject, blood.main.startLifetime.constant));
            return blood;
        }

        public ParticleSystem CreateRchParticle(Vector3 position, int startSpeed, int burstCount)
        {
            var rchParticle = _assets.Instantiate(ParticlePath.RchParticlePath, position, Quaternion.identity).GetComponent<ParticleSystem>();
            var main = rchParticle.main;
            main.startSpeed = startSpeed;
            var burst = new ParticleSystem.Burst(0f, burstCount, 5, 0.05f)
            {
                probability = 1
            };
            
            rchParticle.emission.SetBurst(0, burst);
            _coroutineRunner.StartCoroutine(DestroyParticle(rchParticle.gameObject, main.startLifetime.constant));
            return rchParticle;
        }

        public ParticleSystem CreateFallingMeshParticle(Transform particleContainer)
        {
            return _assets.Instantiate(ParticlePath.FallingMeshParticlePath, particleContainer)
                .GetComponent<ParticleSystem>();
        }

        public ParticleSystem CreateWeatherParticle(string mapName, Transform parent)
        {
            var particles = _staticData.GetMapConfigure(mapName).Weather;
            if (particles != null)
            {
                _assets.Instantiate(particles.gameObject, parent);
            }

            return particles;
        }

        private static IEnumerator DestroyParticle(GameObject particle, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            if (particle != null)
                NetworkServer.Destroy(particle);
        }
    }
}
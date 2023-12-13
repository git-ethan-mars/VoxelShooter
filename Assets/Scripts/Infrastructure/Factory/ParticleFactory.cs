using System.Collections;
using Infrastructure.AssetManagement;
using Infrastructure.Services.StaticData;
using Mirror;
using Particles;
using UnityEngine;

namespace Infrastructure.Factory
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

        public void CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletImpactPath, position, rotation);
            var particleColor = bullet.GetComponent<ParticleColor>();
            particleColor.color = blockColor;
            NetworkServer.Spawn(bullet);
            _coroutineRunner.StartCoroutine(DestroyParticle(bullet,
                bullet.GetComponent<ParticleSystem>().main.startLifetime.constant));
        }

        public void CreateBlood(Vector3 position, Quaternion rotation)
        {
            var blood = _assets.Instantiate(ParticlePath.BloodSprayPath, position, rotation);
            var particleSystem = blood.GetComponent<ParticleSystem>().main;
            NetworkServer.Spawn(blood);
            _coroutineRunner.StartCoroutine(DestroyParticle(blood, particleSystem.startLifetime.constant));
        }

        public void CreateRchParticle(Vector3 position, int startSpeed, int burstCount)
        {
            var rchParticle = _assets.Instantiate(ParticlePath.RchParticlePath, position, Quaternion.identity);
            var particleSystem = rchParticle.GetComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startSpeed = startSpeed;
            var burst = new ParticleSystem.Burst(0f, burstCount, 5, 0.05f)
            {
                probability = 1
            };
            particleSystem.emission.SetBurst(0, burst);
            NetworkServer.Spawn(rchParticle);
            _coroutineRunner.StartCoroutine(DestroyParticle(rchParticle, main.startLifetime.constant));
        }

        public ParticleSystem CreateFallingMeshParticle(Transform particleContainer)
        {
            return _assets.Instantiate(ParticlePath.FallingMeshParticlePath, particleContainer)
                .GetComponent<ParticleSystem>();
        }

        public void CreateWeatherParticle(string mapName, Transform parent)
        {
            var particles = _staticData.GetMapConfigure(mapName).weather;
            if (particles != null)
            {
                _assets.Instantiate(particles.gameObject, parent);
            }
        }

        private static IEnumerator DestroyParticle(GameObject particle, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            if (particle != null)
                NetworkServer.Destroy(particle);
        }
    }
}
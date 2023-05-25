using System.Collections;
using Infrastructure.AssetManagement;
using Mirror;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class ParticleFactory : IParticleFactory
    {
        private readonly IAssetProvider _assets;
        private readonly ICoroutineRunner _coroutineRunner;

        public ParticleFactory(IAssetProvider assets, ICoroutineRunner coroutineRunner)
        {
            _assets = assets;
            _coroutineRunner = coroutineRunner;
        }
        
        public GameObject CreateBulletHole(Vector3 position, Quaternion rotation)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletHolePath, position, rotation);
            NetworkServer.Spawn(bullet); 
            _coroutineRunner.StartCoroutine(DestroyParticle(bullet));
            return bullet;
        }

        public GameObject CreateMuzzleFlash(Transform transform)
        {
            var muzzleFlash = _assets.Instantiate(ParticlePath.MuzzleFlashPath, transform);
            return muzzleFlash;
        }

        public GameObject CreateBlood(Vector3 position)
        {
            var blood = _assets.Instantiate(ParticlePath.BloodSprayPath, position, Quaternion.identity);
            NetworkServer.Spawn(blood);
            _coroutineRunner.StartCoroutine(DestroyParticle(blood));
            return blood;
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
            _coroutineRunner.StartCoroutine(DestroyParticle(rchParticle));
        }

        private static IEnumerator DestroyParticle(GameObject particle)
        {
            yield return new WaitForSeconds(particle.GetComponent<ParticleSystem>().main.startLifetime.constant);
            if (particle != null)
                NetworkServer.Destroy(particle);
        }
    }
}
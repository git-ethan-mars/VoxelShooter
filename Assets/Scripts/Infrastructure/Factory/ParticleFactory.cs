using System.Collections;
using Infrastructure.AssetManagement;
using Mirror;
using Particles;
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
        
        public GameObject CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletHolePath, position, rotation);
            var particleColor = bullet.GetComponent<ParticleColor>();
            particleColor.color = blockColor;
            NetworkServer.Spawn(bullet); 
            _coroutineRunner.StartCoroutine(DestroyParticle(bullet, bullet.GetComponent<ParticleSystem>().main.startLifetime.constant));
            return bullet;
        }

        public GameObject CreateMuzzleFlash(Transform transform)
        {
            var muzzleFlash = _assets.Instantiate(ParticlePath.MuzzleFlashPath, transform);
            return muzzleFlash;
        }

        public GameObject CreateBlood(Vector3 position, Quaternion rotation)
        {
            var blood = _assets.Instantiate(ParticlePath.BloodSprayPath, position, rotation);
            var particleSystem = blood.GetComponent<ParticleSystem>().main;
            NetworkServer.Spawn(blood);
            _coroutineRunner.StartCoroutine(DestroyParticle(blood, particleSystem.startLifetime.constant));
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
            _coroutineRunner.StartCoroutine(DestroyParticle(rchParticle, main.startLifetime.constant));
        }

        public ParticleSystem CreateFallingMeshParticle()
        {
            return _assets.Instantiate(ParticlePath.FallingMeshParticlePath).GetComponent<ParticleSystem>();
        }

        private static IEnumerator DestroyParticle(GameObject particle, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            if (particle != null)
                NetworkServer.Destroy(particle);
        }
    }
}
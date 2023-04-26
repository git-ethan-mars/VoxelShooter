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

        private static IEnumerator DestroyParticle(GameObject particle)
        {
            yield return new WaitForSeconds(particle.GetComponent<ParticleSystem>().main.duration);
            NetworkServer.Destroy(particle);
        }
    }
}
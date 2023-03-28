using Infrastructure.AssetManagement;
using Mirror;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;

        public GameFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public GameObject CreateBulletHole(Vector3 position, Quaternion rotation)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletHolePath, position, rotation);
            NetworkServer.Spawn(bullet);
            return bullet;
        }

        public GameObject CreateMuzzleFlash(Transform transform)
        {
            return _assets.Instantiate(ParticlePath.MuzzleFlashPath, transform);
        }
    }
}
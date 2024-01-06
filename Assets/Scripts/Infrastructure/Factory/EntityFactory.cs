using Data;
using Entities;
using Infrastructure.AssetManagement;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class EntityFactory : IEntityFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IParticleFactory _particleFactory;

        public EntityFactory(IAssetProvider assets, IParticleFactory particleFactory)
        {
            _assets = assets;
            _particleFactory = particleFactory;
        }

        public Tnt CreateTnt(Vector3 position, Quaternion rotation, IServer server,
            NetworkConnectionToClient owner, TntItem tntItem,
            AudioService audioService)
        {
            var tnt = _assets.Instantiate(EntityPath.TntPath, position, rotation).GetComponent<Tnt>();
            tnt.Construct(server, owner, _particleFactory, audioService, tntItem);
            return tnt;
        }

        public Grenade CreateGrenade(Vector3 position, Vector3 force, IServer server,
            NetworkConnectionToClient owner, GrenadeItem grenadeItem,
            AudioService audioService)
        {
            var grenade = _assets.Instantiate(EntityPath.GrenadePath, position, Quaternion.identity)
                .GetComponent<Grenade>();
            grenade.Construct(server, owner, _particleFactory, audioService, grenadeItem);
            grenade.GetComponent<Rigidbody>().AddForce(force);
            return grenade;
        }

        public GameObject CreateTombstone(Vector3 position)
        {
            var tombstone = _assets.Instantiate(EntityPath.TombstonePath, position, Quaternion.identity);
            return tombstone;
        }

        public GameObject CreateRocket(Vector3 position, Quaternion rotation, IServer server,
            NetworkConnectionToClient owner, RocketLauncherItem rocketData,
            AudioService audioService)
        {
            var rocket = _assets.Instantiate(EntityPath.RocketPath, position, rotation);
            rocket.GetComponent<Rocket>().Construct(server, rocketData, owner, _particleFactory, audioService);
            rocket.GetComponent<Rigidbody>().velocity = rocket.transform.forward * rocketData.speed;
            return rocket;
        }

        public LootBox CreateAmmoBox(Vector3 position, Transform parent)
        {
            return CreateLootBox(position, parent, EntityPath.AmmoBoxPath);
        }

        public LootBox CreateHealthBox(Vector3 position, Transform parent)
        {
            return CreateLootBox(position, parent, EntityPath.HealthBoxPath);
        }

        public LootBox CreateBlockBox(Vector3 position, Transform parent)
        {
            return CreateLootBox(position, parent, EntityPath.BlockBoxPath);
        }

        public SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent)
        {
            return _assets.Instantiate(EntityPath.SpawnPointPath, position, Quaternion.identity, parent)
                .GetComponent<SpawnPoint>();
        }

        private LootBox CreateLootBox(Vector3 position, Transform parent, string prefabPath)
        {
            var lootBox = _assets.Instantiate(prefabPath, position, Quaternion.identity, parent)
                .GetComponent<LootBox>();
            return lootBox;
        }
    }
}
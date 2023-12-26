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

        public EntityFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public GameObject CreateTnt(Vector3 position, Quaternion rotation)
        {
            var tnt = _assets.Instantiate(EntityPath.TntPath, position, rotation);
            return tnt;
        }

        public GameObject CreateGrenade(Vector3 position, Quaternion rotation, Vector3 force)
        {
            var grenade = _assets.Instantiate(EntityPath.GrenadePath, position, rotation);
            grenade.GetComponent<Rigidbody>().AddForce(force);
            return grenade;
        }

        public GameObject CreateTombstone(Vector3 position)
        {
            var tombstone = _assets.Instantiate(EntityPath.TombstonePath, position, Quaternion.identity);
            return tombstone;
        }

        public GameObject CreateRocket(Vector3 position, Quaternion rotation, IServer server,
            IParticleFactory particleFactory, RocketLauncherItem rocketData, NetworkConnectionToClient owner,
            AudioService audioService)
        {
            var rocket = _assets.Instantiate(EntityPath.RocketPath, position, rotation);
            rocket.GetComponent<Rocket>().Construct(server, rocketData, owner, particleFactory, audioService);
            rocket.GetComponent<Rigidbody>().velocity = rocket.transform.forward * rocketData.speed;
            return rocket;
        }

        public LootBox CreateAmmoBox(IServer server, Vector3 position, Transform parent)
        {
            return CreateLootBox(server, position, parent, EntityPath.AmmoBoxPath);
        }

        public LootBox CreateHealthBox(IServer server, Vector3 position, Transform parent)
        {
            return CreateLootBox(server, position, parent, EntityPath.HealthBoxPath);
        }

        public LootBox CreateBlockBox(IServer server, Vector3 position, Transform parent)
        {
            return CreateLootBox(server, position, parent, EntityPath.BlockBoxPath);
        }

        public SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent)
        {
            return _assets.Instantiate(EntityPath.SpawnPointPath, position, Quaternion.identity, parent)
                .GetComponent<SpawnPoint>();
        }

        private LootBox CreateLootBox(IServer server, Vector3 position, Transform parent, string prefabPath)
        {
            var lootBox = _assets.Instantiate(prefabPath, position, Quaternion.identity, parent)
                .GetComponent<LootBox>();
            lootBox.Construct(server);
            return lootBox;
        }
    }
}
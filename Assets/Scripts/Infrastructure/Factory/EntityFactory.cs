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
            NetworkServer.Spawn(tnt);
            return tnt;
        }

        public GameObject CreateGrenade(Vector3 position, Quaternion rotation)
        {
            var grenade = _assets.Instantiate(EntityPath.GrenadePath, position, rotation);
            NetworkServer.Spawn(grenade);
            return grenade;
        }

        public GameObject CreateTombstone(Vector3 position)
        {
            var tombstone = _assets.Instantiate(EntityPath.TombstonePath, position, Quaternion.identity);
            NetworkServer.Spawn(tombstone);
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

        public LootBox CreateAmmoBox(Vector3 position, Transform parent)
        {
            var lootBox = _assets.Instantiate(EntityPath.AmmoBoxPath, position, Quaternion.identity, parent);
            NetworkServer.Spawn(lootBox);
            return lootBox.GetComponent<LootBox>();
        }

        public LootBox CreateHealthBox(Vector3 position, Transform parent)
        {
            var lootBox = _assets.Instantiate(EntityPath.HealthBoxPath, position, Quaternion.identity, parent);
            NetworkServer.Spawn(lootBox);
            return lootBox.GetComponent<LootBox>();
        }

        public LootBox CreateBlockBox(Vector3 position, Transform parent)
        {
            var lootBox = _assets.Instantiate(EntityPath.BlockBoxPath, position, Quaternion.identity, parent);
            NetworkServer.Spawn(lootBox);
            return lootBox.GetComponent<LootBox>();
        }

        public SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent)
        {
            return _assets.Instantiate(EntityPath.SpawnPointPath, position, Quaternion.identity, parent)
                .GetComponent<SpawnPoint>();
        }
    }
}
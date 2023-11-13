using Data;
using Entities;
using Infrastructure.AssetManagement;
using Mirror;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class EntityFactory : IEntityFactory
    {
        private readonly IAssetProvider _assets;
        private const string TntPath = "Prefabs/SpawningTnt";
        private const string GrenadePath = "Prefabs/SpawningGrenade";
        private const string RocketPath = "Prefabs/Rocket";
        private const string TombstonePath = "Prefabs/Tombstone";
        private const string SpawnPointPath = "Prefabs/Spawnpoint";
        private const string AmmoBoxPath = "Prefabs/Drops/AmmoBox";
        private const string HealthBoxPath = "Prefabs/Drops/HealthBox";
        private const string BlockBoxPath = "Prefabs/Drops/BlockBox";

        public EntityFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public GameObject CreateTnt(Vector3 position, Quaternion rotation)
        {
            var tnt = _assets.Instantiate(TntPath, position, rotation);
            NetworkServer.Spawn(tnt);
            return tnt;
        }

        public GameObject CreateGrenade(Vector3 position, Quaternion rotation)
        {
            var grenade = _assets.Instantiate(GrenadePath, position, rotation);
            NetworkServer.Spawn(grenade);
            return grenade;
        }

        public GameObject CreateTombstone(Vector3 position)
        {
            var tombstone = _assets.Instantiate(TombstonePath, position, Quaternion.identity);
            NetworkServer.Spawn(tombstone);
            return tombstone;
        }

        public GameObject CreateRocket(Vector3 position, Quaternion rotation, IServer server,
            IParticleFactory particleFactory, RocketLauncherItem rocketData, NetworkConnectionToClient owner)
        {
            var rocket = _assets.Instantiate(RocketPath, position, rotation);
            rocket.GetComponent<Rocket>().Construct(server, rocketData, owner, particleFactory);
            NetworkServer.Spawn(rocket);
            return rocket;
        }
        
        public LootBox CreateAmmoBox(Vector3 position, Transform parent)
        {
            var lootBox = _assets.Instantiate(AmmoBoxPath, position, Quaternion.identity, parent);
            NetworkServer.Spawn(lootBox);
            return lootBox.GetComponent<LootBox>();
        }
        
        public LootBox CreateHealthBox(Vector3 position, Transform parent)
        {
            var lootBox = _assets.Instantiate(HealthBoxPath, position, Quaternion.identity, parent);
            NetworkServer.Spawn(lootBox);
            return lootBox.GetComponent<LootBox>();
        }
        
        public LootBox CreateBlockBox(Vector3 position, Transform parent)
        {
            var lootBox = _assets.Instantiate(BlockBoxPath, position, Quaternion.identity, parent);
            NetworkServer.Spawn(lootBox);
            return lootBox.GetComponent<LootBox>();
        }
        
        public GameObject CreateSpawnPoint(Vector3 position, Transform parent)
        {
            return _assets.Instantiate(SpawnPointPath, position, Quaternion.identity, parent);
        }
    }
}
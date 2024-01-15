using Data;
using Entities;
using Infrastructure.AssetManagement;
using MapLogic;
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

        public Tnt CreateTnt(Vector3 position, Quaternion rotation, TntItem tntItem,
            IServer server,
            NetworkConnectionToClient owner,
            AudioService audioService, Vector3Int linkedPosition)
        {
            var tnt = _assets.Instantiate(EntityPath.TntPath, position, rotation).GetComponent<Tnt>();
            NetworkServer.Spawn(tnt.gameObject, owner);
            tnt.Construct(server, _particleFactory, audioService, tntItem, linkedPosition);
            return tnt;
        }

        public Grenade CreateGrenade(Vector3 position, Vector3 force, GrenadeItem grenadeItem, IServer server,
            NetworkConnectionToClient owner, AudioService audioService)
        {
            var grenade = _assets.Instantiate(EntityPath.GrenadePath, position, Quaternion.identity)
                .GetComponent<Grenade>();
            grenade.GetComponent<Rigidbody>().AddForce(force);
            NetworkServer.Spawn(grenade.gameObject, owner);
            grenade.Construct(server, _particleFactory, audioService, grenadeItem);
            return grenade;
        }

        public GameObject CreateTombstone(Vector3 position, IServer server, NetworkConnectionToClient owner)
        {
            var tombstone = _assets.Instantiate(EntityPath.TombstonePath, position, Quaternion.identity);
            NetworkServer.Spawn(tombstone, owner);
            tombstone.GetComponent<Tombstone>().Construct(server, _particleFactory);
            return tombstone;
        }

        public void CreateRocket(Vector3 position, Quaternion rotation, RocketLauncherItem rocketLauncher,
            IServer server, NetworkConnectionToClient owner, AudioService audioService)
        {
            var rocket = _assets.Instantiate(EntityPath.RocketPath, position, rotation).GetComponent<Rocket>();
            rocket.GetComponent<Rigidbody>().velocity = rocket.transform.forward * rocketLauncher.speed;
            NetworkServer.Spawn(rocket.gameObject, owner);
            rocket.Construct(server, rocketLauncher, _particleFactory, audioService);
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
        
        public GameObject CreateDrill(Vector3 position, Quaternion rotation, DrillItem drillData,
            IServer server, NetworkConnectionToClient owner, AudioService audioService, Vector3 direction)
        {
            var drill = _assets.Instantiate(EntityPath.DrillPath, position, rotation);
            drill.GetComponent<Drill>().Construct(server, drillData, _particleFactory, audioService);
            drill.GetComponent<Rigidbody>().velocity = direction * drillData.speed;
            NetworkServer.Spawn(drill, owner);
            return drill;
        }
    }
}
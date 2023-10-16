using Data;
using Entities;
using Infrastructure.AssetManagement;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class EntityFactory : IEntityFactory
    {
        private readonly IAssetProvider _assets;
        private const string TntPath = "Prefabs/SpawningTnt";
        private const string GrenadePath = "Prefabs/SpawningGrenade";
        private const string RocketPath = "Prefabs/Rocket";
        private const string DrillPath = "Prefabs/Drill";
        private const string TombstonePath = "Prefabs/Tombstone";
        private const string SpawnPointPath = "Prefabs/Spawnpoint";

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

        public GameObject CreateRocket(Vector3 position, Quaternion rotation, MapProvider mapProvider,
            MapUpdater mapUpdater,
            IParticleFactory particleFactory, RocketLauncherItem rocketData, NetworkConnectionToClient owner)
        {
            var rocket = _assets.Instantiate(RocketPath, position, rotation);
            rocket.GetComponent<Rocket>().Construct(mapProvider, mapUpdater, rocketData, owner, particleFactory);
            NetworkServer.Spawn(rocket);
            return rocket;
        }
        
        public GameObject CreateDrill(Vector3 position, Quaternion rotation, MapProvider mapProvider,
            MapUpdater mapUpdater,
            IParticleFactory particleFactory, DrillItem drillData, NetworkConnectionToClient owner,
            ICoroutineRunner coroutineRunner)
        {
            var drill = _assets.Instantiate(DrillPath, position, rotation);
            drill.GetComponent<Drill>().Construct(mapProvider, mapUpdater, drillData, owner, particleFactory, coroutineRunner);
            NetworkServer.Spawn(drill);
            return drill;
        }

        public GameObject CreateSpawnPoint(Vector3 position, Transform parent)
        {
            return _assets.Instantiate(SpawnPointPath, position, Quaternion.identity, parent);
        }
    }
}
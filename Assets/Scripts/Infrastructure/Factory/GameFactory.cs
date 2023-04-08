using Core;
using GamePlay;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
using Mirror;
using Networking;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IMapProvider _mapProvider;
        private MapGenerator _mapGenerator;
        private const string NetworkManagerPath = "Prefabs/Infrastructure/NetworkManager";
        private const string MapGeneratorPath = "Prefabs/MapCreation/MapGenerator";
        private const string ChunkRendererPath = "Prefabs/MapCreation/Chunk";
        private const string PlayerPath = "Prefabs/Player";


        public GameFactory(IAssetProvider assets, IMapProvider mapProvider)
        {
            _assets = assets;
            _mapProvider = mapProvider;
        }


        public GameObject CreatePlayer(Vector3 position, Quaternion rotation)
        {
            var player = _assets.Instantiate(PlayerPath, position, rotation);
            player.GetComponent<MapSynchronization>().Construct(this, _mapProvider, _mapGenerator);
            player.GetComponent<RaycastSynchronization>().Construct(this);
            //player.GetComponent<InventoryController>().Construct(this);
            player.GetComponent<StatSynchronization>().Construct(player.GetComponent<HealthSystem>(), 
                player.GetComponent<InventoryController>().GetGunSystems());
            return player;
        }

        public GameObject CreatePlayer()
        {
            var player = _assets.Instantiate(PlayerPath);
            player.GetComponent<MapSynchronization>().Construct(this, _mapProvider, _mapGenerator);
            player.GetComponent<RaycastSynchronization>().Construct(this);
            //player.GetComponent<InventoryController>().Construct(this);
            player.GetComponent<StatSynchronization>().Construct(player.GetComponent<HealthSystem>(), 
                player.GetComponent<InventoryController>().GetGunSystems());
            return player;
        }

        public GameObject CreateBulletHole(Vector3 position, Quaternion rotation)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletHolePath, position, rotation);
            NetworkServer.Spawn(bullet);
            return bullet;
        }

        public GameObject CreateMuzzleFlash(Transform transform) =>
            _assets.Instantiate(ParticlePath.MuzzleFlashPath, transform);

        public GameObject CreateNetworkManager()
        {
            var networkManager = _assets.Instantiate(NetworkManagerPath);
            networkManager.GetComponent<CustomNetworkManager>().Construct(_mapProvider, this);
            return networkManager;
        }

        public GameObject CreateMapGenerator()
        {
            var mapGenerator = _assets.Instantiate(MapGeneratorPath);
            _mapGenerator = mapGenerator.GetComponent<MapGenerator>();
            _mapGenerator.Construct(_mapProvider, this);
            return mapGenerator;
        }

        public ChunkRenderer CreateChunkRenderer(Vector3Int position, Quaternion rotation, Transform transform)
        {
            var chunkRenderer = _assets.Instantiate(ChunkRendererPath, position, rotation, transform)
                .GetComponent<ChunkRenderer>();
            chunkRenderer.Construct();
            return chunkRenderer;
        }
    }
}
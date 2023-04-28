using System.Collections.Generic;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private MapRenderer _mapGenerator;
        private readonly IStaticDataService _staticData;
        private const string NetworkManagerPath = "Prefabs/Network/LocalNetworkManager";
        private const string SteamNetworkManagerPath = "Prefabs/Network/SteamManager";
        private const string MapSynchronization = "Prefabs/MapCreation/MapSyncronization";
        private const string MapRendererPath = "Prefabs/MapCreation/MapRenderer";
        private const string ChunkRendererPath = "Prefabs/MapCreation/Chunk";
        private const string Wall = "Prefabs/MapCreation/Wall";
        private GameObject _networkManager;
        private readonly IEntityFactory _entityFactory;
        private readonly IParticleFactory _particleFactory;


        public GameFactory(IAssetProvider assets, IEntityFactory entityFactory, IParticleFactory particleFactory, IStaticDataService staticData)
        {
            _assets = assets;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _staticData = staticData;
        }

        public GameObject CreateLocalNetworkManager(bool isLocalBuild)
        {
            _networkManager = _assets.Instantiate(NetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(_staticData, _entityFactory, _particleFactory, _assets, isLocalBuild);
            return _networkManager;
        }

        public GameObject CreateSteamNetworkManager(bool isLocalBuild)
        {
            _networkManager = _assets.Instantiate(SteamNetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(_staticData, _entityFactory, _particleFactory, _assets, isLocalBuild);
            return _networkManager;
        }

        public GameObject CreateMapSynchronization()
        {
            var mapSynchronization = _assets.Instantiate(MapSynchronization);
            NetworkServer.Spawn(mapSynchronization);
            return mapSynchronization;
        }

        public void CreateWalls(Map map)
        {
            _assets.Instantiate(Wall).GetComponent<WallRenderer>().Construct(map, Faces.Top);
            _assets.Instantiate(Wall).GetComponent<WallRenderer>().Construct(map, Faces.Bottom);
            _assets.Instantiate(Wall).GetComponent<WallRenderer>().Construct(map, Faces.Right);
            _assets.Instantiate(Wall).GetComponent<WallRenderer>().Construct(map, Faces.Left);
            _assets.Instantiate(Wall).GetComponent<WallRenderer>().Construct(map, Faces.Front);
            _assets.Instantiate(Wall).GetComponent<WallRenderer>().Construct(map, Faces.Back);
        }

        public GameObject CreateMapRenderer(Map map, Dictionary<Vector3Int, BlockData> buffer)
        {
            var mapGenerator = _assets.Instantiate(MapRendererPath);
            _mapGenerator = mapGenerator.GetComponent<MapRenderer>();
            _mapGenerator.Construct(map, this, buffer);
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
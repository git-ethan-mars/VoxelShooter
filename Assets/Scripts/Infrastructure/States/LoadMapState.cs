using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using MapLogic;
using Networking;
using Networking.Synchronization;
using UnityEngine;

namespace Infrastructure.States
{
    public class LoadMapState : IPayloadedState<string>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IStaticDataService _staticData;
        private readonly IAssetProvider _assets;
        private CustomNetworkManager _networkManager;
        private readonly IParticleFactory _particleFactory;
        private MapMessageHandler mapMessageHandler;
        private readonly bool _isLocalBuild;

        public LoadMapState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory,
            IStaticDataService staticData, IAssetProvider assets, IParticleFactory particleFactory, bool isLocalBuild)
        {
            _sceneLoader = sceneLoader;
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _staticData = staticData;
            _particleFactory = particleFactory;
            _assets = assets;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter(string sceneName)
        {
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        public void Exit()
        {
        }

        private void OnLoaded()
        {
            mapMessageHandler = new MapMessageHandler();
            _networkManager = _isLocalBuild
                ? _gameFactory.CreateLocalNetworkManager(mapMessageHandler).GetComponent<CustomNetworkManager>()
                : _gameFactory.CreateSteamNetworkManager(mapMessageHandler).GetComponent<CustomNetworkManager>();
            _networkManager.MapLoaded += OnMapLoaded;
            mapMessageHandler.MapDownloaded += OnMapDownloaded;
        }
        
        private void OnMapLoaded(Map map)
        {
            Debug.Log("Loaded map from disk");
            mapMessageHandler.Map = map;
            _gameFactory.CreateMapRenderer(map, mapMessageHandler.Buffer);
            CreateServerEntityFactory(map);
            _stateMachine.Enter<GameLoopState>();
        }

        private void OnMapDownloaded(Map map)
        {
            mapMessageHandler.Map = map;
            Debug.Log("Downloaded map from server");
            _gameFactory.CreateMapRenderer(map, mapMessageHandler.Buffer);
            _stateMachine.Enter<GameLoopState>();
        }

        private void CreateServerEntityFactory(Map map)
        {
            var entityFactory = new EntityFactory(map, _assets, _staticData, _networkManager.ServerData, _particleFactory);
            _networkManager.EntityFactory = entityFactory;
        }

    }
}
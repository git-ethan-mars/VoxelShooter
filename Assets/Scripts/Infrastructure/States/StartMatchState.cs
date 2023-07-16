using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class StartMatchState : IPayloadedState<ServerSettings>
    {
        private CustomNetworkManager _networkManager;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private readonly GameStateMachine _stateMachine;
        private const string Main = "Main";

        public StartMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
        }

        public void Enter(ServerSettings serverSettings)
        {
            _sceneLoader.Load(Main, () => CreateHost(serverSettings));
        }

        private void CreateHost(ServerSettings serverSettings)
        {
            _networkManager = _gameFactory.CreateLocalNetworkManager(_stateMachine, serverSettings)
                .GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
            _networkManager.StartHost();
        }

        private void OnMapDownloaded(IMapProvider mapProvider, Dictionary<Vector3Int, BlockData> mapUpdates)
        {
            _gameFactory.CreateWalls(mapProvider);
            _gameFactory.CreateMapRenderer(mapProvider, mapUpdates);
            _stateMachine.Enter<GameLoopState,CustomNetworkManager>(_networkManager);
        }

        public void Exit()
        {
        }
    }
}
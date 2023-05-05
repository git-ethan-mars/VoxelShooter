using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class StartMatchState : IPayloadedState<string, ServerSettings>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private readonly bool _isLocalBuild;
        private CustomNetworkManager _networkManager;
        private readonly GameStateMachine _stateMachine;

        public StartMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory, bool isLocalBuild)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter(string sceneName, ServerSettings serverSettings)
        {
            _sceneLoader.Load(sceneName, () =>CreateHost(serverSettings));
        }

        private void CreateHost(ServerSettings serverSettings)
        {
            _networkManager = _isLocalBuild
                ? _gameFactory.CreateLocalNetworkManager(_stateMachine, _isLocalBuild, serverSettings).GetComponent<CustomNetworkManager>()
                : _gameFactory.CreateSteamNetworkManager(_stateMachine, _isLocalBuild, serverSettings).GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
            _networkManager.StartHost();
        }

        private void OnMapDownloaded(Map map, Dictionary<Vector3Int, BlockData> mapUpdates)
        {
            _gameFactory.CreateWalls(map);
            _gameFactory.CreateMapRenderer(map, mapUpdates);
            _stateMachine.Enter<GameLoopState>();
        }

        public void Exit()
        {
        }
    }
}
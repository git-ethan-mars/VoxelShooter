using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class JoinMatchState : IPayloadedState<string>
    {
        private readonly IGameFactory _gameFactory;
        private readonly bool _isLocalBuild;
        private readonly GameStateMachine _stateMachine;
        private CustomNetworkManager _networkManager;
        private readonly SceneLoader _sceneLoader;


        public JoinMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory,
            bool isLocalBuild)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
            _isLocalBuild = isLocalBuild;
        }


        public void Enter(string sceneName)
        {
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        private void OnLoaded()
        {
            _networkManager = _isLocalBuild
                ? _gameFactory.CreateLocalNetworkManager(_stateMachine, _isLocalBuild, null)
                    .GetComponent<CustomNetworkManager>()
                : _gameFactory.CreateSteamNetworkManager(_stateMachine, _isLocalBuild, null)
                    .GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
            _networkManager.StartClient();
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
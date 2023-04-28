using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class LoadMapState : IPayloadedState<string>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private CustomNetworkManager _networkManager;
        private readonly bool _isLocalBuild;

        public LoadMapState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory, bool isLocalBuild)
        {
            _sceneLoader = sceneLoader;
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
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
            _networkManager = _isLocalBuild
                ? _gameFactory.CreateLocalNetworkManager(_isLocalBuild).GetComponent<CustomNetworkManager>()
                : _gameFactory.CreateSteamNetworkManager(_isLocalBuild).GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
        }

        private void OnMapDownloaded(Map map, Dictionary<Vector3Int, BlockData> mapUpdates)
        {
            _gameFactory.CreateWalls(map);
            _gameFactory.CreateMapRenderer(map, mapUpdates);
            _stateMachine.Enter<GameLoopState>();
        }
        

    }
}
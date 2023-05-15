using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class JoinLocalMatchState : IPayloadedState<string>
    {
        private readonly IGameFactory _gameFactory;
        private readonly bool _isLocalBuild;
        private readonly GameStateMachine _stateMachine;
        private CustomNetworkManager _networkManager;
        private readonly SceneLoader _sceneLoader;


        public JoinLocalMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
        }


        public void Enter(string sceneName)
        {
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        private void OnLoaded()
        {
            _networkManager = _gameFactory.CreateLocalNetworkManager(_stateMachine, null)
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
using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class JoinSteamLobbyState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private CustomNetworkManager _networkManager;
        private IUIFactory _uiFactory;
        private const string Main = "Main";

        public JoinSteamLobbyState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory,
            IUIFactory uiFactory)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
            _uiFactory = uiFactory;
        }

        public void Enter()
        {
            _sceneLoader.Load(Main, OnLoaded);
        }

        private void OnLoaded()
        {
            _networkManager = _gameFactory.CreateSteamNetworkManager(_stateMachine, null, false)
                .GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
            _uiFactory.CreateLoadingWindow(_networkManager);
        }

        private void OnMapDownloaded(Map map, Dictionary<Vector3Int, BlockData> mapUpdates)
        {
            _gameFactory.CreateWalls(map);
            _gameFactory.CreateMapRenderer(map, mapUpdates);
            _stateMachine.Enter<GameLoopState, CustomNetworkManager>(_networkManager);
        }

        public void Exit()
        {
        }
    }
}
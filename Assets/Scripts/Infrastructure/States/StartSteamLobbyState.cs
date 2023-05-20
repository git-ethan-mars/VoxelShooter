using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class StartSteamLobbyState : IPayloadedState<ServerSettings>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private CustomNetworkManager _networkManager;
        private const string Main = "Main";


        public StartSteamLobbyState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory)
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
            _networkManager = _gameFactory.CreateSteamNetworkManager(_stateMachine, serverSettings, true).GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
        }
        
        private void OnMapDownloaded(Map map, Dictionary<Vector3Int, BlockData> mapUpdates)
        {
            _gameFactory.CreateWalls(map);
            _gameFactory.CreateMapRenderer(map, mapUpdates);
            _stateMachine.Enter<GameLoopState,CustomNetworkManager>(_networkManager);
        }

        public void Exit()
        {
        }
    }
}
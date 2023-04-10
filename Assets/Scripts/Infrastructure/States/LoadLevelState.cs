using Infrastructure.Factory;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking;
using UnityEngine;

namespace Infrastructure.States
{
    public class LoadMapState : IPayloadedState<string>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;

        public LoadMapState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory)
        {
            _sceneLoader = sceneLoader;
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
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
            var networkManager = _gameFactory.CreateNetworkManager();
            networkManager.GetComponent<NetworkManagerHUD>().OnButtonDown +=
                OnStartGame;
            networkManager.GetComponent<CustomNetworkManager>().ConnectionHappened +=
                () => _stateMachine.Enter<GameLoopState>();
        }

        private void OnStartGame()
        {
            var mapGenerator = _gameFactory.CreateMapGenerator();
            AllServices.Container.RegisterSingle<IMapGeneratorProvider>(new MapGeneratorProvider(mapGenerator.GetComponent<MapGenerator>()));
        }
    }
}
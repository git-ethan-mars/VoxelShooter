using Infrastructure.Factory;
using MapLogic;
using Networking;

namespace Infrastructure.States
{
    public class JoinSteamLobbyState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private readonly IUIFactory _uiFactory;
        private CustomNetworkManager _networkManager;
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
            _networkManager = _gameFactory.CreateSteamNetworkManager(null, false);
            _networkManager.MapLoaded += OnMapLoaded;
            _uiFactory.CreateLoadingWindow(_networkManager);
        }

        private void OnMapLoaded(IMapProvider mapProvider)
        {
            _networkManager.MapLoaded -= OnMapLoaded;
            _stateMachine.Enter<GameLoopState, CustomNetworkManager>(_networkManager);
        }

        public void Exit()
        {
        }
    }
}
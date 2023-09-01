using Infrastructure.Factory;
using Networking;

namespace Infrastructure.States
{
    public class JoinSteamLobbyState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private CustomNetworkManager _networkManager;
        private readonly IUIFactory _uiFactory;
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

        private void OnMapDownloaded()
        {
            _gameFactory.CreateWalls(_networkManager.Client.MapProvider);
            _gameFactory.CreateMapRenderer(_networkManager.Client);
            _stateMachine.Enter<GameLoopState, CustomNetworkManager>(_networkManager);
        }

        public void Exit()
        {
        }
    }
}
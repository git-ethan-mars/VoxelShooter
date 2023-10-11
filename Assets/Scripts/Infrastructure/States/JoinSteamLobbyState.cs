using Infrastructure.Factory;
using Networking;

namespace Infrastructure.States
{
    public class JoinSteamLobbyState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
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
            var networkManager = _gameFactory.CreateSteamNetworkManager(_stateMachine, null, false)
                .GetComponent<CustomNetworkManager>();
            _uiFactory.CreateLoadingWindow(networkManager);
        }

        public void Exit()
        {
        }
    }
}
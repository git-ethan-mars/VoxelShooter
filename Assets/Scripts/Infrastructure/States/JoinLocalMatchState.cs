using Infrastructure.Factory;
using Networking;

namespace Infrastructure.States
{
    public class JoinLocalMatchState : IState
    {
        private readonly IGameFactory _gameFactory;
        private readonly bool _isLocalBuild;
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IUIFactory _uiFactory;
        private const string Main = "Main";


        public JoinLocalMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory,
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
            var networkManager = _gameFactory.CreateLocalNetworkManager(_stateMachine, null)
                .GetComponent<CustomNetworkManager>();
            networkManager.StartClient();
            _uiFactory.CreateLoadingWindow(networkManager.Client);
        }

        public void Exit()
        {
        }
    }
}
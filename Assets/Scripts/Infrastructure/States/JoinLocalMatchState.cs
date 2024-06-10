using Infrastructure.Factory;
using MapLogic;
using Networking;

namespace Infrastructure.States
{
    public class JoinLocalMatchState : IState
    {
        private readonly IGameFactory _gameFactory;
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IUIFactory _uiFactory;
        private CustomNetworkManager _networkManager;
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
            _networkManager = _gameFactory.CreateLocalNetworkManager(null)
                .GetComponent<CustomNetworkManager>();
            _networkManager.MapLoaded += OnMapLoaded;
            _networkManager.StartClient();
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
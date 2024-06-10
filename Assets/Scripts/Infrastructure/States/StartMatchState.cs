using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;

namespace Infrastructure.States
{
    public class StartMatchState : IPayloadedState<ServerSettings>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private readonly GameStateMachine _stateMachine;
        private CustomNetworkManager _networkManager;
        private const string Main = "Main";

        public StartMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory)
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
            _networkManager = _gameFactory.CreateLocalNetworkManager(serverSettings)
                .GetComponent<CustomNetworkManager>();
            _networkManager.MapLoaded += OnMapLoaded;
            _networkManager.StartHost();
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
using Data;
using Infrastructure.Factory;
using Networking;

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
            _networkManager = _gameFactory.CreateSteamNetworkManager(_stateMachine, serverSettings, true)
                .GetComponent<CustomNetworkManager>();
            _networkManager.MapDownloaded += OnMapDownloaded;
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
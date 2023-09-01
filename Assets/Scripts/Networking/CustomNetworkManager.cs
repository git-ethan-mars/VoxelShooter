using System;
using Data;
using Generators;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using MapLogic;
using Mirror;
using Networking.ClientServices;
using Networking.ServerServices;
using MemoryStream = System.IO.MemoryStream;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager, ICoroutineRunner
    {
        public event Action GameFinished;
        private const string SpawnPointContainerName = "SpawnPointContainer";
        public Client Client;
        private IStaticDataService _staticData;
        private IEntityFactory _entityFactory;
        private ServerSettings _serverSettings;
        private GameStateMachine _stateMachine;
        private ServerTimer _serverTimer;
        private IParticleFactory _particleFactory;
        private IMeshFactory _meshFactory;
        private IAssetProvider _assets;
        private IGameFactory _gameFactory;
        private IServer _server;
        private const float ShowResultsDuration = 10;


        public void Construct(GameStateMachine stateMachine, IStaticDataService staticData,
            IEntityFactory entityFactory, IParticleFactory particleFactory, IAssetProvider assets,
            IGameFactory gameFactory, IMeshFactory meshFactory,
            ServerSettings serverSettings)
        {
            _stateMachine = stateMachine;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _assets = assets;
            _gameFactory = gameFactory;
            _meshFactory = meshFactory;
            _serverSettings = serverSettings;
        }

        public override void OnStartServer()
        {
            _server = new Server(this, _staticData, _serverSettings, _assets, _particleFactory, _entityFactory);
            _server.RegisterHandlers();
            var spawnPointContainer = _gameFactory.CreateGameObjectContainer(SpawnPointContainerName);
            _server.CreateSpawnPoints(spawnPointContainer.transform);
            _serverTimer = new ServerTimer(this, _serverSettings.MaxDuration, StopHost);
            _serverTimer.Start();
        }


        public override void OnStartClient()
        {
            Client = new Client(_gameFactory, _meshFactory);
            Client.RegisterHandlers();
            Client.Data.State = ClientState.Connecting;
            Client.MapDownloaded += OnMapDownloaded;
        }

        public override void OnServerReady(NetworkConnectionToClient connection)
        {
            base.OnServerReady(connection);
            if (IsHost(connection))
            {
                Client.MapProvider = _server.MapProvider;
                OnMapDownloaded();
            }
            else
            {
                var memoryStream = new MemoryStream();
                MapWriter.WriteMap(_server.MapProvider, memoryStream);
                var bytes = memoryStream.ToArray();
                var mapSplitter = new MapSplitter();
                var mapMessages = mapSplitter.SplitBytesIntoMessages(bytes, 100000);
                StartCoroutine(mapSplitter.SendMessages(mapMessages, connection, 1f));
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient connection)
        {
            base.OnServerDisconnect(connection);
            _server.DeletePlayer(connection);
        }

        public override void OnStopClient()
        {
            Client.UnregisterHandlers();
            Client.Data.State = ClientState.NotConnected;
            if (NetworkClient.activeHost) return;
            _gameFactory.CreateCamera();
            GameFinished?.Invoke();
            StartCoroutine(Utils.DoActionAfterDelay(ShowResultsDuration,
                _stateMachine.Enter<MainMenuState>));
        }

        public override void OnStopServer()
        {
            _server.UnregisterHandlers();
            _gameFactory.CreateCamera();
            GameFinished?.Invoke();
            StartCoroutine(Utils.DoActionAfterDelay(ShowResultsDuration,
                _stateMachine.Enter<MainMenuState>));
        }

        private void OnMapDownloaded()
        {
            Client.MapDownloaded -= OnMapDownloaded;
            Client.Data.State = ClientState.Connected;
            Client.MapGenerator = new MapGenerator(Client);
            _stateMachine.Enter<GameLoopState, CustomNetworkManager>(this);
        }

        private static bool IsHost(NetworkConnection conn) =>
            NetworkClient.connection.connectionId == conn.connectionId;
    }
}
using System;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using UnityEngine;
using MemoryStream = System.IO.MemoryStream;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager, ICoroutineRunner
    {
        public event Action<IMapProvider, Dictionary<Vector3Int, BlockData>> MapDownloaded;
        public event Action<ServerTime> ServerTimeChanged;
        public event Action<ServerTime> RespawnTimeChanged;
        public event Action<List<ScoreData>> ScoreboardChanged;
        public event Action<float> OnLoadProgress;
        public event Action GameFinished;
        private IStaticDataService _staticData;
        private IEntityFactory _entityFactory;
        private ClientMessagesHandler _clientMessageHandlers;
        private ServerServices.ServerMessageHandlers _serverMessageHandlers;
        private ServerSettings _serverSettings;
        private GameStateMachine _stateMachine;
        private ServerTimer _serverTimer;
        private IParticleFactory _particleFactory;
        private IAssetProvider _assets;
        private IGameFactory _gameFactory;
        private IServer _server;
        private const float ShowResultsDuration = 10;


        public void Construct(GameStateMachine stateMachine, IStaticDataService staticData,
            IEntityFactory entityFactory, IParticleFactory particleFactory, IAssetProvider assets,
            IGameFactory gameFactory,
            ServerSettings serverSettings)
        {
            _stateMachine = stateMachine;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _assets = assets;
            _gameFactory = gameFactory;
            _serverSettings = serverSettings;
        }

        public override void OnStartServer()
        {
            _server = new Server(this, _staticData, _serverSettings, _assets, _particleFactory, _entityFactory);
            _serverMessageHandlers =
                new ServerMessageHandlers(_entityFactory, this, _server, _staticData, _particleFactory);
            _serverMessageHandlers.RegisterHandlers();
            _serverTimer = new ServerTimer(this, _serverSettings.MaxDuration, StopHost);
            _serverTimer.Start();
        }


        public override void OnStartClient()
        {
            _clientMessageHandlers = new ClientMessagesHandler();
            _clientMessageHandlers.RegisterHandlers();
            _clientMessageHandlers.MapDownloaded +=
                (mapProvider, mapUpdates) => MapDownloaded?.Invoke(mapProvider, mapUpdates);
            _clientMessageHandlers.ServerTimeUpdated += timeLeft => ServerTimeChanged?.Invoke(timeLeft);
            _clientMessageHandlers.RespawnTimeUpdated += timeLeft => RespawnTimeChanged?.Invoke(timeLeft);
            _clientMessageHandlers.ScoreboardUpdated += scores => ScoreboardChanged?.Invoke(scores);
            _clientMessageHandlers.MapProgress += progress => OnLoadProgress?.Invoke(progress);
        }


        public override void OnServerReady(NetworkConnectionToClient connection)
        {
            base.OnServerReady(connection);
            if (IsHost(connection))
            {
                MapDownloaded?.Invoke(_server.MapProvider, _clientMessageHandlers.MapUpdates);
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

        public override void OnClientDisconnect()
        {
        }

        public override void OnServerDisconnect(NetworkConnectionToClient connection)
        {
            base.OnServerDisconnect(connection);
            _server.DeletePlayer(connection);
        }

        public override void OnStopClient()
        {
            _clientMessageHandlers.RemoveHandlers();
            if (NetworkClient.activeHost) return;
            _gameFactory.CreateCamera();
            GameFinished?.Invoke();
            StartCoroutine(Utils.DoActionAfterDelay(ShowResultsDuration,
                _stateMachine.Enter<MainMenuState>));
        }

        public override void OnStopServer()
        {
            _serverMessageHandlers.RemoveHandlers();
            _gameFactory.CreateCamera();
            GameFinished?.Invoke();
            StartCoroutine(Utils.DoActionAfterDelay(ShowResultsDuration,
                _stateMachine.Enter<MainMenuState>));
        }


        private static bool IsHost(NetworkConnection conn) =>
            NetworkClient.connection.connectionId == conn.connectionId;
    }
}
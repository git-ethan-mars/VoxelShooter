using System;
using System.Collections.Generic;
using System.IO;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager, ICoroutineRunner
    {
        public event Action<Map, Dictionary<Vector3Int, BlockData>> MapDownloaded;
        public event Action<ServerTime> ServerTimeChanged;
        public event Action<ServerTime> RespawnTimeChanged;
        public event Action<List<ScoreData>> ScoreboardChanged; 
        private ServerData _serverData;
        private IStaticDataService _staticData;
        private IEntityFactory _entityFactory;
        private ClientMessagesHandler _clientMessageHandlers;
        private ServerMessageHandlers _serverMessageHandlers;
        private ServerSettings _serverSettings;
        private GameStateMachine _stateMachine;
        private ServerTimer _serverTimer;
        private IParticleFactory _particleFactory;
        private IAssetProvider _assets;


        public void Construct(GameStateMachine stateMachine, IStaticDataService staticData,
            IEntityFactory entityFactory, IParticleFactory particleFactory, IAssetProvider assets, ServerSettings serverSettings)
        {
            _stateMachine = stateMachine;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _assets = assets;
            _serverSettings = serverSettings;
        }

        public override void OnStartServer()
        {
            _serverData = new ServerData(this, _assets, _staticData, _particleFactory,
                MapReader.ReadFromFile(_serverSettings.MapName + ".rch"), _serverSettings);
            _serverMessageHandlers =
                new ServerMessageHandlers(_entityFactory, this, _serverData);
            _serverMessageHandlers.RegisterHandlers();
            _serverTimer = new ServerTimer(this, _serverSettings.MaxDuration, _stateMachine.Enter<MainMenuState>);
            _serverTimer.Start();
        }


        public override void OnStartClient()
        {
            _clientMessageHandlers = new ClientMessagesHandler();
            _clientMessageHandlers.RegisterHandlers();
            _clientMessageHandlers.MapDownloaded += (map, mapUpdates) => MapDownloaded?.Invoke(map, mapUpdates);
            _clientMessageHandlers.ServerTimeUpdated += timeLeft => ServerTimeChanged?.Invoke(timeLeft);
            _clientMessageHandlers.RespawnTimeUpdated += timeLeft => RespawnTimeChanged?.Invoke(timeLeft);
            _clientMessageHandlers.ScoreboardUpdated += scores => ScoreboardChanged?.Invoke(scores);
        }


        public override void OnServerReady(NetworkConnectionToClient connection)
        {
            base.OnServerReady(connection);
            if (IsHost(connection))
            {
                MapDownloaded?.Invoke(_serverData.Map, _clientMessageHandlers.MapUpdates);
            }

            else
            {
                var memoryStream = new MemoryStream();
                MapWriter.WriteMap(_serverData.Map, memoryStream);
                var bytes = memoryStream.ToArray();
                var mapSplitter = new MapSplitter();
                var mapMessages = mapSplitter.SplitBytesIntoMessages(bytes, 100000);
                StartCoroutine(mapSplitter.SendMessages(mapMessages, connection, 1f));
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient connection)
        {
            base.OnServerDisconnect(connection);
            _serverData.DeletePlayer(connection);
        }

        public override void OnStopClient()
        {
            _clientMessageHandlers.RemoveHandlers();
            if (!NetworkClient.activeHost)
                _stateMachine.Enter<MainMenuState>();
        }

        public override void OnStopServer()
        {
            _serverMessageHandlers.RemoveHandlers();
            _stateMachine.Enter<MainMenuState>();
        }

        private static bool IsHost(NetworkConnection conn) =>
            NetworkClient.connection.connectionId == conn.connectionId;

        public override void OnDestroy()
        {
            if (NetworkClient.isConnected)
            {
                StopClient();
            }

            if (NetworkServer.active)
            {
                StopServer();
            }
        }
    }
}
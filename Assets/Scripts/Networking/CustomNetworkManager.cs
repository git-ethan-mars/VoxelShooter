using System;
using System.Collections.Generic;
using System.IO;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.States;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager, ICoroutineRunner
    {
        public event Action<Map, Dictionary<Vector3Int, BlockData>> MapDownloaded; 
        private ServerData _serverData;
        private IStaticDataService _staticData;
        private IEntityFactory _entityFactory;
        private IAssetProvider _assets;
        private IParticleFactory _particleFactory;
        private ClientMessagesHandler _clientMessageHandlers;
        private ServerMessageHandlers _serverMessageHandlers;
        private ServerSettings _serverSettings;
        private GameStateMachine _stateMachine;
        private bool _isLocalBuild;


        public void Construct(GameStateMachine stateMachine, IStaticDataService staticData, IEntityFactory entityFactory,
            IParticleFactory particleFactory, IAssetProvider assets, ServerSettings serverSettings, bool isLocalBuild)
        {
            _stateMachine = stateMachine;
            _assets = assets;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _serverSettings = serverSettings;
            _isLocalBuild = isLocalBuild;
        }

        public override void OnStartServer()
        {
            _serverData = new ServerData(_staticData, MapReader.ReadFromFile(_serverSettings.MapName + ".rch"));
            var playerFactory = new PlayerFactory(this, _assets, _staticData, _serverData, _serverSettings, _particleFactory);
            _serverMessageHandlers =
                new ServerMessageHandlers(_entityFactory, this, _serverData, playerFactory, _isLocalBuild);
            _serverMessageHandlers.RegisterHandlers();
        }


        public override void OnStartClient()
        {
            _clientMessageHandlers = new ClientMessagesHandler();
            _clientMessageHandlers.RegisterHandlers();
            _clientMessageHandlers.MapDownloaded += (map, mapUpdates)=>MapDownloaded?.Invoke(map, mapUpdates);
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
                StartCoroutine(mapSplitter.SendMessages(mapMessages, connection, 0.1f));
            }
        }
        
        public override void OnStopClient()
        {
            _clientMessageHandlers.RemoveHandlers();
            if (!NetworkClient.activeHost)
                _stateMachine.Enter<MainMenuState, string>("MainMenu");
        }

        public override void OnStopServer()
        {
            _serverMessageHandlers.RemoveHandlers();
            _stateMachine.Enter<MainMenuState, string>("MainMenu");
        }
        
        private static bool IsHost(NetworkConnection conn) => NetworkClient.connection.connectionId == conn.connectionId;
    }
}
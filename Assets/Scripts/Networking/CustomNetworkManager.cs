using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Mirror;
using Networking.ServerServices;
using UnityEngine;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager, ICoroutineRunner
    {
        public IClient Client;
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
            Client = new Client(_stateMachine, this, _gameFactory, _meshFactory, _staticData, _particleFactory);
        }

        public override void OnStartHost()
        {
            _server = new Server(this, _staticData, _serverSettings, _assets, _gameFactory, _particleFactory,
                _entityFactory);
            _server.Start();
            _serverTimer = new ServerTimer(this, _serverSettings.MaxDuration, StopHost);
            _serverTimer.Start();
        }

        public override void OnStartClient()
        {
            Client.Start();
        }

        public override void OnServerReady(NetworkConnectionToClient connection)
        {
            if (NetworkServer.activeHost)
            {
                Client.MapProvider = _server.MapProvider;
                NetworkServer.SetClientReady(connection);
            }
            else
            {
                _server.SendMap(connection);
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient connection)
        {
            _server.DeletePlayer(connection);
        }

        public override void OnStopClient()
        {
            Client.Stop();
        }

        public override void OnStopServer()
        {
            _server.Stop();
        }
    }
}
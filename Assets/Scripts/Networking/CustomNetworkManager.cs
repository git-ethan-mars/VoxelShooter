using System.Globalization;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Mirror;
using Networking.Messages.Requests;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager, ICoroutineRunner
    {
        public IClient Client { get; private set; }
        public IAssetProvider Assets { get; private set; }
        public IStaticDataService StaticData { get; private set; }
        public IStorageService StorageService { get; private set; }
        public IInputService InputService { get; private set; }
        public IGameFactory GameFactory { get; private set; }
        public IEntityFactory EntityFactory { get; private set; }
        public IMeshFactory MeshFactory { get; private set; }
        public IParticleFactory ParticleFactory { get; private set; }
        public IPlayerFactory PlayerFactory { get; private set; }
        private ServerSettings _serverSettings;
        private GameStateMachine _stateMachine;
        private IServer _server;


        public void Construct(GameStateMachine stateMachine, IInputService inputService, IStorageService storageService,
            IStaticDataService staticData,
            IEntityFactory entityFactory, IParticleFactory particleFactory, IGameFactory gameFactory,
            IMeshFactory meshFactory, IUIFactory uiFactory, IAssetProvider assets,
            ServerSettings serverSettings)
        {
            _stateMachine = stateMachine;
            _serverSettings = serverSettings;
            Assets = assets;
            StaticData = staticData;
            StorageService = storageService;
            InputService = inputService;
            GameFactory = gameFactory;
            EntityFactory = entityFactory;
            MeshFactory = meshFactory;
            ParticleFactory = particleFactory;
            PlayerFactory = new PlayerFactory(assets, inputService, storageService, staticData, uiFactory, meshFactory);
            Client = new Client(_stateMachine, this);
        }

        public override void OnStartHost()
        {
            _server = new Server(this, _serverSettings);
            _server.Start();
        }

        public override void OnStartClient()
        {
            Client.Start();
        }

        public override void OnServerReady(NetworkConnectionToClient connection)
        {
            if (NetworkServer.localConnection == connection)
            {
                Client.MapProvider = _server.MapProvider;
                Client.MapName = _server.MapProvider.MapName;
                Client.PrefabRegistrar.LootBoxes = _server.EntityContainer.LootBoxes;
            }
            else
            {
                _server.SendCurrentServerState(connection);
            }

            NetworkServer.SetClientReady(connection);
        }

        public override void OnClientConnect()
        {
            NetworkClient.Ready();
            NetworkClient.Send(new AuthenticationRequest(Constants.isLocalBuild ? CSteamID.Nil : SteamUser.GetSteamID(),
                Constants.isLocalBuild
                    ? Random.value.ToString(CultureInfo.InvariantCulture)
                    : SteamFriends.GetPersonaName()));
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
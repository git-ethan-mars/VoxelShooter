using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private const string NetworkManagerPath = "Prefabs/Network/LocalNetworkManager";
        private const string SteamNetworkManagerPath = "Prefabs/Network/SteamManager";
        private GameObject _networkManager;
        private readonly IEntityFactory _entityFactory;
        private readonly IAvatarLoader _avatarLoader;
        private readonly IPlayerFactory _playerFactory;
        private readonly IParticleFactory _particleFactory;
        private readonly IMeshFactory _meshFactory;

        public GameFactory(IAssetProvider assets, IEntityFactory entityFactory,
            IStaticDataService staticData, IParticleFactory particleFactory, IMeshFactory meshFactory)
        {
            _assets = assets;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _meshFactory = meshFactory;
            _staticData = staticData;
        }

        public GameObject CreateLocalNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings)
        {
            _networkManager = _assets.Instantiate(NetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(stateMachine, _staticData, _entityFactory,
                _particleFactory, _assets, this, _meshFactory, serverSettings);
            return _networkManager;
        }

        public GameObject CreateSteamNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings,
            bool isHost)
        {
            _networkManager = _assets.Instantiate(SteamNetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(stateMachine, _staticData, _entityFactory,
                _particleFactory, _assets, this, _meshFactory, serverSettings);
            _networkManager.GetComponent<SteamLobby>().Construct(isHost);
            return _networkManager;
        }

        public GameObject CreateGameObjectContainer(string containerName)
        {
            return new GameObject(containerName);
        }

        public void CreateCamera()
        {
            new GameObject().AddComponent<Camera>();
        }
    }
}
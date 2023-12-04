using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private const string NetworkManagerPath = "Prefabs/Network/LocalNetworkManager";
        private const string SteamNetworkManagerPath = "Prefabs/Network/SteamManager";
        private const string DirectionalLightName = "Directional Light";
        private readonly IAssetProvider _assets;
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;
        private readonly IStaticDataService _staticData;
        private readonly IEntityFactory _entityFactory;
        private readonly IAvatarLoader _avatarLoader;
        private readonly IPlayerFactory _playerFactory;
        private readonly IParticleFactory _particleFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IUIFactory _uiFactory;

        public GameFactory(IAssetProvider assets, IInputService inputService, IStorageService storageService,
            IEntityFactory entityFactory,
            IStaticDataService staticData, IParticleFactory particleFactory, IMeshFactory meshFactory,
            IUIFactory uiFactory)
        {
            _assets = assets;
            _inputService = inputService;
            _storageService = storageService;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _meshFactory = meshFactory;
            _staticData = staticData;
            _uiFactory = uiFactory;
        }

        public CustomNetworkManager CreateLocalNetworkManager(GameStateMachine stateMachine,
            ServerSettings serverSettings)
        {
            var networkManager = _assets.Instantiate(NetworkManagerPath).GetComponent<CustomNetworkManager>();
            networkManager.Construct(stateMachine, _inputService, _storageService, _staticData,
                _entityFactory,
                _particleFactory, _assets, this, _meshFactory, _uiFactory, serverSettings);
            return networkManager;
        }

        public CustomNetworkManager CreateSteamNetworkManager(GameStateMachine stateMachine,
            ServerSettings serverSettings,
            bool isHost)
        {
            var networkManager = _assets.Instantiate(SteamNetworkManagerPath).GetComponent<CustomNetworkManager>();
            networkManager.Construct(stateMachine, _inputService, _storageService, _staticData,
                _entityFactory,
                _particleFactory, _assets, this, _meshFactory, _uiFactory, serverSettings);
            networkManager.GetComponent<SteamLobby>().Construct(isHost);
            return networkManager;
        }

        public GameObject CreateGameObjectContainer(string containerName)
        {
            return new GameObject(containerName);
        }

        public void CreateCamera()
        {
            new GameObject().AddComponent<Camera>();
        }

        public void CreateDirectionalLight(LightData lightData)
        {
            var light = new GameObject(DirectionalLightName).AddComponent<Light>();
            light.transform.position = lightData.position;
            light.transform.rotation = lightData.rotation;
            light.color = lightData.color;
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.shadowNormalBias = 0;
        }
    }
}
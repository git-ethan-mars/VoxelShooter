using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
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
        private const string AudioSourcePath = "Prefabs/AudioSource";
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

        public GameFactory(AllServices allServices)
        {
            _assets = allServices.Single<IAssetProvider>();
            _inputService = allServices.Single<IInputService>();
            _storageService = allServices.Single<IStorageService>();
            _entityFactory = allServices.Single<IEntityFactory>();
            _particleFactory = allServices.Single<IParticleFactory>();
            _meshFactory = allServices.Single<IMeshFactory>();
            _staticData = allServices.Single<IStaticDataService>();
            _uiFactory = allServices.Single<IUIFactory>();
        }

        public CustomNetworkManager CreateLocalNetworkManager(GameStateMachine stateMachine,
            ServerSettings serverSettings)
        {
            var networkManager = _assets.Instantiate(NetworkManagerPath).GetComponent<CustomNetworkManager>();
            networkManager.Construct(stateMachine, _inputService, _storageService, _staticData,
                _entityFactory,
                _particleFactory, this, _meshFactory, _uiFactory, _assets, serverSettings);
            return networkManager;
        }

        public CustomNetworkManager CreateSteamNetworkManager(GameStateMachine stateMachine,
            ServerSettings serverSettings,
            bool isHost)
        {
            var networkManager = _assets.Instantiate(SteamNetworkManagerPath).GetComponent<CustomNetworkManager>();
            networkManager.Construct(stateMachine, _inputService, _storageService, _staticData,
                _entityFactory,
                _particleFactory, this, _meshFactory, _uiFactory, _assets, serverSettings);
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

        public AudioSource CreateAudioSource(Transform container)
        {
            return _assets.Instantiate(AudioSourcePath, container).GetComponent<AudioSource>();
        }
    }
}
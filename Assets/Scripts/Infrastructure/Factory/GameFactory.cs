using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
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
        private readonly IInputService _inputService;
        private readonly IStaticDataService _staticData;
        private const string NetworkManagerPath = "Prefabs/Network/LocalNetworkManager";
        private const string SteamNetworkManagerPath = "Prefabs/Network/SteamManager";
        private const string DirectionalLightName = "Directional Light";
        private GameObject _networkManager;
        private readonly IEntityFactory _entityFactory;
        private readonly IAvatarLoader _avatarLoader;
        private readonly IPlayerFactory _playerFactory;
        private readonly IParticleFactory _particleFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IUIFactory _uiFactory;

        public GameFactory(IAssetProvider assets, IInputService inputService, IEntityFactory entityFactory,
            IStaticDataService staticData, IParticleFactory particleFactory, IMeshFactory meshFactory,
            IUIFactory uiFactory)
        {
            _assets = assets;
            _inputService = inputService;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _meshFactory = meshFactory;
            _staticData = staticData;
            _uiFactory = uiFactory;
        }

        public GameObject CreateLocalNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings)
        {
            _networkManager = _assets.Instantiate(NetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(stateMachine, _inputService, _staticData,
                _entityFactory,
                _particleFactory, _assets, this, _meshFactory, _uiFactory, serverSettings);
            return _networkManager;
        }

        public GameObject CreateSteamNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings,
            bool isHost)
        {
            _networkManager = _assets.Instantiate(SteamNetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(stateMachine, _inputService, _staticData,
                _entityFactory,
                _particleFactory, _assets, this, _meshFactory, _uiFactory, serverSettings);
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
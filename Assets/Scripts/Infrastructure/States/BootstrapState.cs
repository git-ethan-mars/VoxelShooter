using Common;
using Common.AssetManagement;
using GamePlay;
using GamePlay.Entities;
using GamePlay.Factory;
using GamePlay.MapFeatures;
using GamePlay.Services;
using Networking;
using UI;
using VoxelMap;

namespace Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _allServices;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader,
            AllServices allServices)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _allServices = allServices;
            RegisterServices();
        }

        public void Enter()
        {
            _sceneLoader.Load(Initial, EnterLoadLevel);
        }

        public void Exit()
        {
        }

        private void EnterLoadLevel()
        {
            _stateMachine.Enter<MainMenuState>();
        }

        private void RegisterServices()
        {
            var assetProvider = new AssetProvider();
            _allServices.RegisterSingle<IAssetProvider>(assetProvider);
            _allServices.RegisterSingle<IStaticDataService>(
                new StaticDataService(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IStorageService>(new JsonToFileStorageService());
            var staticData = _allServices.Single<IStaticDataService>();
            _allServices.RegisterSingle<IMapConfigureLoader>(
                new MapConfigureLoader(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IMapRepository>(new MapRepository(_allServices.Single<IMapConfigureLoader>()));
            staticData.LoadItems();
            staticData.LoadInventories();
            staticData.LoadPlayerCharacteristics();
            staticData.LoadLobbyBalance();
            staticData.LoadVoxelHealthBalance();
            staticData.LoadSounds();
            staticData.LoadFallDamageConfiguration();
            staticData.LoadRectPaletteData();
            _allServices.RegisterSingle<IInputService>(new StandaloneInputService());
# if LOCAL_BUILD
            _allServices.RegisterSingle<IAvatarLoader>(
                new LocalAvatarLoader(_allServices.Single<IAssetProvider>()));
#else
			_allServices.RegisterSingle<IAvatarLoader>(
				new SteamAvatarLoader(_allServices.Single<IAssetProvider>()));
#endif
            _allServices.RegisterSingle<IParticleFactory>(new ParticleFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IStaticDataService>()));
            _allServices.RegisterSingle<IMeshFactory>(new MeshFactory(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IUIFactory>(new UIFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IStaticDataService>()));
            _allServices.RegisterSingle<IEntityFactory>(new EntityFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IParticleFactory>()));
            _allServices.RegisterSingle<IInventoryFactory>(new InventoryFactory(_allServices.Single<IAssetProvider>(),
                staticData, _allServices.Single<IEntityFactory>(), _allServices.Single<IInputService>()));
            _allServices.RegisterSingle<ICharacterFactory>(new NetworkCharacterFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IStorageService>(), _allServices.Single<IInputService>(), staticData));
            _allServices.RegisterSingle<INetworkingFactory>(new NetworkingFactory(_allServices.Single<IAssetProvider>(),
                staticData, _allServices.Single<IStorageService>(),
                _allServices.Single<ICharacterFactory>(), _allServices.Single<IEntityFactory>(),
                _allServices.Single<IParticleFactory>(), _allServices.Single<IMeshFactory>(),
                _allServices.Single<IInventoryFactory>(), _allServices.Single<IMapConfigureLoader>()));
        }
    }
}
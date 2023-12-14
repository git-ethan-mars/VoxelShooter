using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;

namespace Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _allServices;
        private readonly ICoroutineRunner _coroutineRunner;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices allServices,
            ICoroutineRunner coroutineRunner)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _allServices = allServices;
            _coroutineRunner = coroutineRunner;
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
            _allServices.RegisterSingle<IAssetProvider>(new AssetProvider());
            _allServices.RegisterSingle<IStaticDataService>(
                new StaticDataService(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IStorageService>(new JsonToFileStorageService());
            var staticData = _allServices.Single<IStaticDataService>();
            _allServices.RegisterSingle<IMapRepository>(new MapRepository(staticData));
            staticData.LoadItems();
            staticData.LoadInventories();
            staticData.LoadPlayerCharacteristics();
            staticData.LoadMapConfigures();
            staticData.LoadLobbyBalance();
            staticData.LoadBlockHealthBalance();
            staticData.LoadSounds();
            staticData.LoadFallDamageConfiguration();
            _allServices.RegisterSingle<IInputService>(new StandaloneInputService());
            if (Constants.isLocalBuild)
            {
                _allServices.RegisterSingle<IAvatarLoader>(
                    new LocalAvatarLoader(_allServices.Single<IAssetProvider>()));
            }
            else
            {
                _allServices.RegisterSingle<IAvatarLoader>(
                    new SteamAvatarLoader(_allServices.Single<IAssetProvider>()));
            }

            _allServices.RegisterSingle<IParticleFactory>(new ParticleFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IStaticDataService>(),
                _coroutineRunner));
            _allServices.RegisterSingle<IMeshFactory>(new MeshFactory(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IEntityFactory>(new EntityFactory(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IUIFactory>(new UIFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IStaticDataService>()));
            _allServices.RegisterSingle<IGameFactory>(
                new GameFactory(_allServices));
        }
    }
}
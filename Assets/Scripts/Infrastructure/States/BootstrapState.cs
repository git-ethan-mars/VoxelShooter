using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;

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
            _allServices.RegisterSingle<IStaticDataService>(new StaticDataService());
            var staticData = _allServices.Single<IStaticDataService>();
            _allServices.RegisterSingle<IMapRepository>(new MapRepository(staticData));
            staticData.LoadItems();
            staticData.LoadInventories();
            staticData.LoadPlayerCharacteristics();
            staticData.LoadMapConfigures();
            _allServices.RegisterSingle<IInputService>(new StandaloneInputService());
            _allServices.RegisterSingle<IAssetProvider>(new AssetProvider());
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
                _coroutineRunner));
            _allServices.RegisterSingle<IMeshFactory>(new MeshFactory(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IEntityFactory>(new EntityFactory(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IUIFactory>(new UIFactory(_allServices.Single<IAssetProvider>(),
                _allServices.Single<IStaticDataService>()));
            _allServices.RegisterSingle<IGameFactory>(
                new GameFactory(_allServices.Single<IAssetProvider>(), _allServices.Single<IInputService>(),
                    _allServices.Single<IEntityFactory>(),
                    staticData, _allServices.Single<IParticleFactory>(), _allServices.Single<IMeshFactory>(),
                    _allServices.Single<IUIFactory>()));
        }
    }
}
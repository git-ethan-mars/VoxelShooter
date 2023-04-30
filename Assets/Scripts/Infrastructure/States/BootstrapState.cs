using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.Input;

namespace Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _allServices;
        private ICoroutineRunner _coroutineRunner;

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
            _stateMachine.Enter<LoadMapState, string>("Main");
        }

        private void RegisterServices()
        {
            _allServices.RegisterSingle<IStaticDataService>(new StaticDataService());
            var staticData = _allServices.Single<IStaticDataService>();
            staticData.LoadItems();
            staticData.LoadInventories();
            staticData.LoadPlayerCharacteristics();
            _allServices.RegisterSingle<IInputService>(new StandaloneInputService());
            _allServices.RegisterSingle<IAssetProvider>(new AssetProvider());
            _allServices.RegisterSingle<IParticleFactory>(new ParticleFactory(_allServices.Single<IAssetProvider>(), _coroutineRunner));
            _allServices.RegisterSingle<IEntityFactory>(new EntityFactory(_allServices.Single<IAssetProvider>()));
            _allServices.RegisterSingle<IGameFactory>(
                new GameFactory(_allServices.Single<IAssetProvider>(), _allServices.Single<IEntityFactory>(), _allServices.Single<IParticleFactory>(), staticData));
            _allServices.RegisterSingle<IUIFactory>(new UIFactory(_allServices.Single<IAssetProvider>(), _allServices.Single<IInputService>()));
        }
    }
}
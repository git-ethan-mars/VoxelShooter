using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using MapLogic;

namespace Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _allServices;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices allServices)
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
            _stateMachine.Enter<LoadMapState, string>("Main");
        }

        private void RegisterServices()
        {
            _allServices.RegisterSingle<IStaticDataService>(new StaticDataService());
            var staticData = _allServices.Single<IStaticDataService>();
            staticData.LoadItems();
            staticData.LoadInventories();
            staticData.LoadPlayerCharacteristics();
            _allServices.RegisterSingle<IMapProvider>(new MapProvider(Map.CreateNewMap(32,32,32)));
            _allServices.RegisterSingle<IInputService>(new StandaloneInputService());
            _allServices.RegisterSingle<IAssetProvider>(new AssetProvider());
            _allServices.RegisterSingle<IGameFactory>(
                new GameFactory(_allServices.Single<IAssetProvider>(),_allServices.Single<IInputService>(), _allServices.Single<IMapProvider>(), _allServices.Single<IStaticDataService>()));
        }
    }
}
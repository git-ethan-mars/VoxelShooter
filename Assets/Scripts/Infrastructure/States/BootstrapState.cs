using Common;
using Common.AssetManagement;
using Common.Audio;
using Common.Factory;
using Common.Input;
using Common.Services;
using Common.Services.StaticData;
using Common.StaticData;
using Common.Storage;
using Entities;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.PlayerDataLoader;
using Networking;

namespace Infrastructure.States
{
	public class BootstrapState : IState
	{
		private const string Initial = "Initial";
		private readonly GameStateMachine _stateMachine;
		private readonly ICoroutineRunner _coroutineRunner;
		private readonly SceneLoader _sceneLoader;
		private readonly AllServices _allServices;

		public BootstrapState(GameStateMachine stateMachine, ICoroutineRunner coroutineRunner, SceneLoader sceneLoader,
			AllServices allServices)
		{
			_stateMachine = stateMachine;
			_coroutineRunner = coroutineRunner;
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
# if LOCAL_BUILD
				_allServices.RegisterSingle<IAvatarLoader>(
					new LocalAvatarLoader(_allServices.Single<IAssetProvider>()));
#else
			_allServices.RegisterSingle<IAvatarLoader>(
				new SteamAvatarLoader(_allServices.Single<IAssetProvider>()));
#endif
			_allServices.RegisterSingle<IAudioPlayer>(new AudioPlayer( _allServices.Single<IAssetProvider>()));
			_allServices.RegisterSingle<IParticleFactory>(new ParticleFactory(_allServices.Single<IAssetProvider>(), _allServices.Single<IStaticDataService>(), _coroutineRunner));
			_allServices.RegisterSingle<IMeshFactory>(new MeshFactory(_allServices.Single<IAssetProvider>()));
			_allServices.RegisterSingle<IUIFactory>(new UIFactory(_allServices.Single<IAssetProvider>(),
				_allServices.Single<IStaticDataService>()));
			_allServices.RegisterSingle<IEntityFactory>(new EntityFactory(_allServices.Single<IAssetProvider>(),
				_allServices.Single<IStorageService>(), _allServices.Single<IInputService>(),
				_allServices.Single<IStaticDataService>(), _allServices.Single<IMeshFactory>(),
				_allServices.Single<IParticleFactory>(), _allServices.Single<IAudioPlayer>()));
			_allServices.RegisterSingle<INetworkingFactory>(new NetworkingFactory(_allServices.Single<IAssetProvider>(), staticData, _allServices.Single<IStorageService>(),
				_allServices.Single<IEntityFactory>(),
				_allServices.Single<IParticleFactory>(), _allServices.Single<IMeshFactory>()));
		}
	}
}
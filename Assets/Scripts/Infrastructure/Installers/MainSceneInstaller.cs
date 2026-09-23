using GamePlay;
using GamePlay.Audio;
using Infrastructure.States;
using Networking;
using R3;
using Reflex.Core;
using Services;
using UI;
using UI.Inventory;
using UnityEngine;
using VoxelMap;
namespace Infrastructure.Installers
{
	public class MainSceneInstaller : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder containerBuilder)
		{
			BindCommonServices(containerBuilder);
			BindFactories(containerBuilder);
			BindGameObjects(containerBuilder);
			BindRegistry(containerBuilder);
			
			containerBuilder.AddSingleton(typeof(LootBoxSpawner));
			containerBuilder.AddSingleton(typeof(SpawnPointService));
			containerBuilder.AddSingleton(typeof(RespawnService));
			containerBuilder.AddSingleton(typeof(KillList));

			Observable.FromEvent<Container>(
					handler => containerBuilder.OnContainerBuilt += handler,
					handler => containerBuilder.OnContainerBuilt -= handler)
				.Subscribe(OnContainerBuilt).AddTo(this);
		}

		private void BindFactories(ContainerBuilder containerBuilder)
		{
			containerBuilder.AddSingleton(typeof(ItemFactory), typeof(IItemFactory));
			containerBuilder.AddSingleton(typeof(EntityFactory), typeof(IEntityFactory));
			containerBuilder.AddSingleton(typeof(NetworkFactory), typeof(INetworkFactory));
			containerBuilder.AddSingleton(typeof(ParticleFactory), typeof(IParticleFactory));
			containerBuilder.AddSingleton(typeof(MeshFactory), typeof(IMeshFactory));
			containerBuilder.AddSingleton(typeof(SlotPresenterFactory), typeof(ISlotPresenterFactory));
			containerBuilder.AddSingleton(typeof(GameModeFactory));
		}

		private void BindCommonServices(ContainerBuilder containerBuilder)
		{
			containerBuilder.AddSingleton(typeof(CharacterProvider));
			containerBuilder.AddSingleton(typeof(MapProvider));
			containerBuilder.AddSingleton(typeof(CameraProvider));
			containerBuilder.AddSingleton(typeof(EntityContainer));
			containerBuilder.AddSingleton(typeof(AudioPlayer));
			containerBuilder.AddSingleton(typeof(NetworkAudioSender));
# if LOCAL_BUILD
			containerBuilder.AddSingleton(typeof(LocalPlayerDataLoader), typeof(IPlayerDataLoader));
#else
			containerBuilder.AddSingleton(typeof(SteamPlayerDataLoader), typeof(IPlayerDataLoader));
#endif
		}

		private void BindRegistry(ContainerBuilder containerBuilder)
		{
			containerBuilder.AddSingleton(typeof(SlotPresenterRegistry), typeof(ISlotPresenterRegistry));
		}

		private void BindGameObjects(ContainerBuilder containerBuilder)
		{
			containerBuilder.AddSingleton(container => container.Single<INetworkFactory>().CreateNetworkManager());
		}

		private void OnContainerBuilt(Container container)
		{
			RegisterGameStates(container);
			RegisterItemPresenters(container);
		}

		private static void RegisterGameStates(Container container)
		{
			var gameStateMachine = container.Single<GameStateMachine>();
			gameStateMachine.RegisterState(container.Construct<InitializeHostState>());
			gameStateMachine.RegisterState(container.Construct<InitializeClientState>());
			gameStateMachine.RegisterState(container.Construct<JoinSteamLobbyState>());
			gameStateMachine.RegisterState(container.Construct<GameLoopState>());
		}

		private static void RegisterItemPresenters(Container container)
		{
			var staticData = container.Single<IStaticDataService>();
			var cameraService = container.Single<CameraProvider>();
			var uiProvider = container.Single<UIProvider>();
			var characterProvider = container.Single<CharacterProvider>();
			var registry = container.Single<ISlotPresenterRegistry>();
			registry.Register<Block>((block, view) =>
				new BlockPresenter(staticData, characterProvider, uiProvider, block, view));
			registry.Register<DrillLauncher>((drill, view) =>
				new DrillLauncherPresenter(staticData, uiProvider, drill, view));
			registry.Register<Grenade>((grenade, view) =>
				new GrenadePresenter(staticData, uiProvider, grenade, view));
			registry.Register<MeleeWeapon>((melee, view) =>
				new MeleeWeaponPresenter(staticData, uiProvider, melee, view));
			registry.Register<RangeWeapon>((weapon, view) =>
				new RangeWeaponPresenter(staticData, uiProvider, cameraService, view, weapon));
			registry.Register<RocketLauncher>((rocket, view) =>
				new RocketLauncherPresenter(staticData, uiProvider, rocket, view));
			registry.Register<TNT>((tnt, view) =>
				new TNTPresenter(staticData, uiProvider, tnt, view));
			registry.Register<Blueprint>((blueprint, view) =>
				new BlueprintPresenter(staticData, characterProvider, uiProvider, blueprint, view));
		}
	}
}
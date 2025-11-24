using Infrastructure.States;
using R3;
using Reflex.Core;
using Services;
using Services.ServerList;
using UI;
using UnityEngine;
namespace Infrastructure.Installers
{
	public class ProjectInstaller : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder containerBuilder)
		{
			BindServices(containerBuilder);
			BindFactories(containerBuilder);
			containerBuilder.AddSingleton(typeof(AssetProvider), typeof(IAssetProvider));
			containerBuilder.AddSingleton(typeof(SceneLoader));
			containerBuilder.AddSingleton(typeof(MapConfigureLoader), typeof(IMapConfigureLoader));
			containerBuilder.AddSingleton(typeof(GameStateMachine));
			Observable.FromEvent<Container>(
					handler => containerBuilder.OnContainerBuilt += handler,
					handler => containerBuilder.OnContainerBuilt -= handler)
				.Subscribe(OnContainerBuilt).AddTo(this);
		}

		private void BindServices(ContainerBuilder containerBuilder)
		{
			containerBuilder.AddSingleton(typeof(StaticDataService), typeof(IStaticDataService));
			containerBuilder.AddSingleton(typeof(JsonToFileStorageService), typeof(IStorageService));
			containerBuilder.AddSingleton(typeof(StandaloneInputService), typeof(IInputService));
			containerBuilder.AddSingleton(typeof(ServerListService), typeof(IServerListService));
		}

		private void BindFactories(ContainerBuilder containerBuilder)
		{
			containerBuilder.AddSingleton(typeof(UIFactory), typeof(IUIFactory));
		}

		private void OnContainerBuilt(Container container)
		{
			container.Single<IStaticDataService>().Initialize();
			var gameStateMachine = container.Single<GameStateMachine>();
			gameStateMachine.RegisterState(container.Construct<BootstrapState>());
			gameStateMachine.RegisterState(container.Construct<GameMenuState>());
		}
	}
}
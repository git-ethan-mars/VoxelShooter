using System;
using System.Collections.Generic;
using Common;
using Common.AssetManagement;
using Common.Input;
using Common.Services;
using Common.Services.StaticData;
using Common.StaticData;
using Common.Storage;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.PlayerDataLoader;
using Networking;

namespace Infrastructure.States
{
	public class GameStateMachine
	{
		private readonly Dictionary<Type, IExitableState> _states;
		private IExitableState _activeState;

		public GameStateMachine(SceneLoader sceneLoader, ICoroutineRunner coroutineRunner, AllServices allServices)
		{
			_states = new Dictionary<Type, IExitableState>
			{
				[typeof(BootstrapState)] =
					new BootstrapState(this, coroutineRunner, sceneLoader, allServices),
				[typeof(MainMenuState)] =
					new MainMenuState(this, sceneLoader, allServices.Single<IUIFactory>(),
						allServices.Single<IStorageService>()),
				[typeof(SettingsMenuState)] = new SettingsMenuState(this, allServices.Single<IUIFactory>(),
					allServices.Single<IStorageService>()),
				[typeof(CreateMatchState)] = new CreateMatchState(this, allServices.Single<IMapRepository>(),
					allServices.Single<IUIFactory>()),
				[typeof(StartSteamLobbyState)] =
					new StartSteamLobbyState(sceneLoader),
				[typeof(JoinSteamLobbyState)] = new JoinSteamLobbyState(this, sceneLoader, allServices.Single<IUIFactory>()),
				[typeof(StartMatchState)] =
					new StartMatchState(this, sceneLoader, allServices.Single<INetworkingFactory>()),
				[typeof(JoinLocalMatchState)] =
					new JoinLocalMatchState(this, sceneLoader, allServices.Single<INetworkingFactory>(),
						allServices.Single<IUIFactory>()),
				[typeof(GameLoopState)] =
					new GameLoopState(this, coroutineRunner, allServices.Single<IAssetProvider>(),
						allServices.Single<IStaticDataService>(), allServices.Single<IUIFactory>(),
						allServices.Single<IInputService>(),
						allServices.Single<IStorageService>(),
						allServices.Single<IAvatarLoader>())
			};
		}

		public void Enter<TState>() where TState : class, IState
		{
			var state = ChangeState<TState>();
			state.Enter();
		}

		public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
		{
			var state = ChangeState<TState>();
			state.Enter(payload);
		}

		private TState ChangeState<TState>() where TState : class, IExitableState
		{
			_activeState?.Exit();
			TState state = GetState<TState>();
			_activeState = state;
			return state;
		}

		private TState GetState<TState>() where TState : class, IExitableState
		{
			return _states[typeof(TState)] as TState;
		}
	}
}
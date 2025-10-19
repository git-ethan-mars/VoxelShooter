using Data;
using ParrelSync;
using R3;
using Services;
using UI;
using UnityEngine;
namespace Infrastructure.States
{
	public class GameMenuState : IState
	{
		private readonly SceneLoader _sceneLoader;
		private readonly GameStateMachine _stateMachine;
		private readonly IStorageService _storageService;
		private readonly IUIFactory _uiFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;

		public GameMenuState(GameStateMachine stateMachine, SceneLoader sceneLoader, IUIFactory uiFactory, IStorageService storageService, 
			IMapConfigureLoader mapConfigureLoader)
		{
			_sceneLoader = sceneLoader;
			_uiFactory = uiFactory;
			_storageService = storageService;
			_stateMachine = stateMachine;
			_mapConfigureLoader = mapConfigureLoader;
		}

		public async void Enter()
		{
			await _sceneLoader.LoadAsync(Scenes.GameMenu);

			Cursor.lockState = CursorLockMode.None;
			AudioListener.volume = _storageService.Load<VolumeSettingsData>(IStorageService.VolumeSettingsKey).MasterVolume;

			GameMenu gameMenu = _uiFactory.CreateGameMenu();
			gameMenu.CreateGameRequested.Subscribe(OnCreateGameRequested).AddTo(gameMenu);
			if (!ClonesManager.IsClone())
			{
				//OnCreateGameRequested(new WorldSettings("Test", _mapConfigureLoader.GetMapConfigure("Test"), 10, 10, 10));
				OnCreateGameRequested(new WorldSettings("Crossroads", _mapConfigureLoader.GetMapConfigure("Crossroads"), 10, 10, 10));
			}
			else
			{
				OnJoinButtonPressed();
			}
#if LOCAL_BUILD
			gameMenu.JoinButtonPressed.Subscribe(_ => OnJoinButtonPressed()).AddTo(gameMenu);
#endif
		}

		public void Exit()
		{
		}

		private async void OnCreateGameRequested(WorldSettings worldSettings)
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			_stateMachine.Enter<InitializeHostState, WorldSettings>(worldSettings);
		}

#if LOCAL_BUILD
		private async void OnJoinButtonPressed()
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			_stateMachine.Enter<InitializeClientState>();
		}
#endif
	}
}
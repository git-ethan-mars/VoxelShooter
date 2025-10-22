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
		private readonly GameStateMachine _gameStateMachine;
		private readonly IStorageService _storageService;
		private readonly IUIFactory _uiFactory;

		public GameMenuState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, IUIFactory uiFactory, IStorageService storageService)
		{
			_sceneLoader = sceneLoader;
			_uiFactory = uiFactory;
			_storageService = storageService;
			_gameStateMachine = gameStateMachine;
		}

		public async void Enter()
		{
			await _sceneLoader.LoadAsync(Scenes.GameMenu);

			Cursor.lockState = CursorLockMode.None;
			AudioListener.volume = _storageService.Load<VolumeSettingsData>(IStorageService.VolumeSettingsKey).MasterVolume;

			GameMenu gameMenu = _uiFactory.CreateGameMenu();
			gameMenu.CreateGameRequested.Subscribe(OnCreateGameRequested).AddTo(gameMenu);
			if (ClonesManager.IsClone())
			{
				//OnCreateGameRequested(new WorldSettings("Test", _mapConfigureLoader.GetMapConfigure("Test"), 10, 10, 10));
				OnCreateGameRequested(new GameSettings("Crossroads", 10, 10, 10));
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

		private async void OnCreateGameRequested(GameSettings gameSettings)
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			_gameStateMachine.Enter<InitializeHostState, GameSettings>(gameSettings);
		}

#if LOCAL_BUILD
		private async void OnJoinButtonPressed()
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			_gameStateMachine.Enter<InitializeClientState>();
		}
#endif
	}
}
using Data;
using R3;
using Services;
using Services.ServerList;
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

#if LOCAL_BUILD
			gameMenu.JoinButtonPressed.Subscribe(_ => OnJoinButtonPressed()).AddTo(gameMenu);
#endif
			gameMenu.JoinServerButtonPressed.Subscribe(OnJoinServerButtonPressed).AddTo(gameMenu);
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
		private async void OnJoinServerButtonPressed(Server server)
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			_gameStateMachine.Enter<JoinSteamLobbyState, Server>(server);
		}
	}
}
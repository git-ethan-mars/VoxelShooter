using Cysharp.Threading.Tasks;
using Data;
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

		public async UniTask EnterAsync()
		{
			await _sceneLoader.LoadAsync(Scenes.GameMenu);

			Cursor.lockState = CursorLockMode.None;
			AudioListener.volume = _storageService.Load<VolumeSettingsData>(IStorageService.VolumeSettingsKey).MasterVolume;

			GameMenu gameMenu = _uiFactory.CreateGameMenu();
			gameMenu.CreateGameRequested.Subscribe(gameSettings => OnCreateGameRequested(gameSettings).Forget()).AddTo(gameMenu);

#if LOCAL_BUILD
			gameMenu.JoinButtonPressed.Subscribe(_ => OnJoinButtonPressed().Forget()).AddTo(gameMenu);
#else
			gameMenu.JoinServerButtonPressed.Subscribe(server => OnJoinServerButtonPressed(server).Forget()).AddTo(gameMenu);
#endif
		}

		public void Exit()
		{
		}

		private async UniTaskVoid OnCreateGameRequested(GameSettings gameSettings)
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			await _gameStateMachine.EnterAsync<InitializeHostState, GameSettings>(gameSettings);
		}

#if LOCAL_BUILD
		private async UniTaskVoid OnJoinButtonPressed()
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			await _gameStateMachine.EnterAsync<InitializeClientState>();
		}
#endif
		private async UniTaskVoid OnJoinServerButtonPressed(Server server)
		{
			await _sceneLoader.LoadAsync(Scenes.Main);
			await _gameStateMachine.EnterAsync<JoinSteamLobbyState, Server>(server);
		}
	}
}

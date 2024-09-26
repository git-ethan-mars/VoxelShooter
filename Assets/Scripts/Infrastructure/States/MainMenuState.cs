using Common;
using Common.Storage;
using Infrastructure.Factory;
using UI;
using UnityEngine;

namespace Infrastructure.States
{
	public class MainMenuState : IState
	{
		private readonly SceneLoader _sceneLoader;
		private readonly IUIFactory _uiFactory;
		private readonly IStorageService _storageService;
		private readonly GameStateMachine _stateMachine;
		private MainMenu _mainMenu;
		private const string MainMenu = "MainMenu";

		public MainMenuState(GameStateMachine stateMachine, SceneLoader sceneLoader, IUIFactory uiFactory,
			IStorageService storageService)
		{
			_sceneLoader = sceneLoader;
			_uiFactory = uiFactory;
			_storageService = storageService;
			_stateMachine = stateMachine;
		}

		public void Enter()
		{
			Cursor.lockState = CursorLockMode.None;
			AudioListener.volume = _storageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey).MasterVolume;
			_sceneLoader.Load(MainMenu, EnterLoadLevel);
		}

		private void EnterLoadLevel()
		{
			_mainMenu = _uiFactory.CreateMainMenu();
			_mainMenu.CreateMatchButtonPressed += _stateMachine.Enter<CreateMatchState>;
			_mainMenu.SettingButtonPressed += _stateMachine.Enter<SettingsMenuState>;
#if LOCAL_BUILD
			_mainMenu.JoinButtonPressed += _stateMachine.Enter<JoinLocalMatchState>;
#else           
            _mainMenu.JoinButtonPressed += _stateMachine.Enter<JoinSteamLobbyState>;
#endif
			_mainMenu.ExitButtonPressed += Application.Quit;
		}

		public void Exit()
		{
			_mainMenu.CreateMatchButtonPressed -= _stateMachine.Enter<CreateMatchState>;
			_mainMenu.SettingButtonPressed -= _stateMachine.Enter<SettingsMenuState>;
#if LOCAL_BUILD
			_mainMenu.JoinButtonPressed -= _stateMachine.Enter<JoinLocalMatchState>;
#else
			_mainMenu.JoinButtonPressed -= _stateMachine.Enter<JoinSteamLobbyState>;
#endif
			_mainMenu.ExitButtonPressed -= Application.Quit;
			Object.Destroy(_mainMenu.gameObject);
		}
	}
}
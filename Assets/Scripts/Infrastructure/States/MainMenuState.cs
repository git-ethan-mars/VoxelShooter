using Infrastructure.Factory;
using Infrastructure.Services.Storage;
using UI.SettingsMenu;
using UnityEngine;

namespace Infrastructure.States
{
    public class MainMenuState : IState
    {
        private readonly SceneLoader _sceneLoader;
        private readonly IUIFactory _uiFactory;
        private readonly IStorageService _storageService;
        private readonly GameStateMachine _stateMachine;
        private GameObject _mainMenu;
        private const string MainMenu = "MainMenu";

        public MainMenuState(GameStateMachine stateMachine, SceneLoader sceneLoader, IUIFactory uiFactory, IStorageService storageService)
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
            _mainMenu = _uiFactory.CreateMainMenu(_stateMachine);
        }

        public void Exit()
        {
            Object.Destroy(_mainMenu);
        }
    }
}
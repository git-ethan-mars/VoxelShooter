using Infrastructure.Factory;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Mirror;
using UnityEngine;

namespace UI.InGameUI.States
{
    public class InGameMenuState : IInGameUIState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly InGameUIStateMachine _inGameUIStateMachine;
        private readonly NetworkManager _networkManager;
        private readonly InGameMenu _inGameMenu;
        private readonly GameObject _settingsMenu;

        public InGameMenuState(GameStateMachine gameStateMachine, InGameUIStateMachine inGameUIStateMachine,
            IUIFactory uiFactory,
            IStorageService storageService,
            NetworkManager networkManager, InGameMenu inGameMenu)
        {
            _gameStateMachine = gameStateMachine;
            _inGameUIStateMachine = inGameUIStateMachine;
            _settingsMenu =
                uiFactory.CreateSettingsMenu(storageService, HideSettingsMenu);
            HideSettingsMenu();
            _networkManager = networkManager;
            _inGameMenu = inGameMenu;
        }

        public void Enter()
        {
            Cursor.lockState = CursorLockMode.None;
            _inGameMenu.CanvasGroup.alpha = 1.0f;
            _inGameMenu.CanvasGroup.interactable = true;
            _inGameMenu.CanvasGroup.blocksRaycasts = true;
            _inGameMenu.ResumeButton.onClick.AddListener(_inGameUIStateMachine.SwitchState<DefaultState>);
            _inGameMenu.SettingsButton.onClick.AddListener(ShowSettingsMenu);
            _inGameMenu.ExitButton.onClick.AddListener(ExitToMainMenu);
        }

        public void Exit()
        {
            _inGameMenu.CanvasGroup.alpha = 0.0f;
            _inGameMenu.CanvasGroup.interactable = false;
            _inGameMenu.CanvasGroup.blocksRaycasts = false;
            HideSettingsMenu();
            _inGameMenu.ResumeButton.onClick.RemoveListener(_inGameUIStateMachine.SwitchState<DefaultState>);
            _inGameMenu.SettingsButton.onClick.RemoveListener(ShowSettingsMenu);
            _inGameMenu.ExitButton.onClick.RemoveListener(ExitToMainMenu);
        }

        private void ShowSettingsMenu()
        {
            _settingsMenu.SetActive(true);
        }

        private void HideSettingsMenu()
        {
            if (_settingsMenu != null)
            {
                _settingsMenu.SetActive(false);
            }
        }

        private void ExitToMainMenu()
        {
            if (NetworkClient.activeHost)
            {
                _networkManager.StopHost();
            }
            else
            {
                _networkManager.StopClient();
            }

            _gameStateMachine.Enter<MainMenuState>();
        }
    }
}
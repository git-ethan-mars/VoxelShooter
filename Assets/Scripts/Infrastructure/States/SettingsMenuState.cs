using GamePlay.Services;
using UI;
using UnityEngine;

namespace Infrastructure.States
{
    public class SettingsMenuState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IUIFactory _uiFactory;
        private readonly IStorageService _storageService;
        private SettingsMenu _settingsMenu;

        public SettingsMenuState(GameStateMachine stateMachine, IUIFactory uiFactory, IStorageService storageService)
        {
            _stateMachine = stateMachine;
            _uiFactory = uiFactory;
            _storageService = storageService;
        }

        public void Enter()
        {
            _settingsMenu = _uiFactory.CreateSettingsMenu(_storageService, null);
            _settingsMenu.BackMousePressed += _stateMachine.Enter<MainMenuState>;
        }

        public void Exit()
        {
            _settingsMenu.BackMousePressed -= _stateMachine.Enter<MainMenuState>;
            Object.Destroy(_settingsMenu.gameObject);
        }
    }
}
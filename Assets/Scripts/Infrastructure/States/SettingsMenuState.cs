using Infrastructure.Factory;
using Infrastructure.Services.Storage;
using UnityEngine;

namespace Infrastructure.States
{
    public class SettingsMenuState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IUIFactory _uiFactory;
        private readonly IStorageService _storageService;
        private GameObject _settingsMenu;

        public SettingsMenuState(GameStateMachine stateMachine, IUIFactory uiFactory, IStorageService storageService)
        {
            _stateMachine = stateMachine;
            _uiFactory = uiFactory;
            _storageService = storageService;
        }

        public void Enter()
        {
            _settingsMenu = _uiFactory.CreateSettingsMenu(_stateMachine, _storageService);
        }

        public void Exit()
        {
            Object.Destroy(_settingsMenu);
        }
    }
}
using Infrastructure.Factory;
using UnityEngine;

namespace Infrastructure.States
{
    public class SettingsMenuState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IUIFactory _uiFactory;
        private GameObject _settingsMenu;

        public SettingsMenuState(GameStateMachine stateMachine, IUIFactory uiFactory)
        {
            _stateMachine = stateMachine;
            _uiFactory = uiFactory;
        }

        public void Enter()
        {
            _settingsMenu = _uiFactory.CreateSettingsMenu(_stateMachine);
        }

        public void Exit()
        {
            Object.Destroy(_settingsMenu);
        }
    }
}
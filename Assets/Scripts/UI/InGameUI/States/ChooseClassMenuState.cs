using UnityEngine;

namespace UI.InGameUI.States
{
    public class ChooseClassMenuState : IInGameUIState
    {
        private readonly InGameUIStateMachine _uiStateMachine;
        private readonly ChooseClassMenu _chooseClassMenu;

        public ChooseClassMenuState(InGameUIStateMachine uiStateMachine, ChooseClassMenu chooseClassMenu)
        {
            _uiStateMachine = uiStateMachine;
            _chooseClassMenu = chooseClassMenu;
        }

        public void Enter()
        {
            Cursor.lockState = CursorLockMode.None;
            _chooseClassMenu.CanvasGroup.alpha = 1.0f;
            _chooseClassMenu.CanvasGroup.interactable = true;
            _chooseClassMenu.CanvasGroup.blocksRaycasts = true;
            _chooseClassMenu.BuilderButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.SniperButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.CombatantButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.GrenadierButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.ExitButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
        }

        public void Exit()
        {
            _chooseClassMenu.CanvasGroup.alpha = 0.0f;
            _chooseClassMenu.CanvasGroup.interactable = false;
            _chooseClassMenu.CanvasGroup.blocksRaycasts = false;
            _chooseClassMenu.BuilderButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.SniperButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.CombatantButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.GrenadierButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.ExitButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
        }
    }
}
using GamePlay.Data;
using UnityEngine;

namespace UI.InGameUIStates
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
			_chooseClassMenu.ChangeClassButtonPressed += OnChangeClassButtonPressed;
		}

		public void Exit()
		{
			_chooseClassMenu.CanvasGroup.alpha = 0.0f;
			_chooseClassMenu.CanvasGroup.interactable = false;
			_chooseClassMenu.CanvasGroup.blocksRaycasts = false;
			_chooseClassMenu.ChangeClassButtonPressed -= OnChangeClassButtonPressed;
		}

		private void OnChangeClassButtonPressed(GameClass gameClass)
		{
			_uiStateMachine.SwitchState<DefaultState>();
		}
	}
}
using UnityEngine;

namespace UI.InGameUIStates
{
	public class InGameMenuState : IInGameUIState
	{
		private readonly InGameMenu _inGameMenu;

		public InGameMenuState(InGameMenu inGameMenu)
		{
			_inGameMenu = inGameMenu;
		}

		public void Enter()
		{
			Cursor.lockState = CursorLockMode.None;
			_inGameMenu.CanvasGroup.alpha = 1.0f;
			_inGameMenu.CanvasGroup.interactable = true;
			_inGameMenu.CanvasGroup.blocksRaycasts = true;
		}

		public void Exit()
		{
			_inGameMenu.CanvasGroup.alpha = 0.0f;
			_inGameMenu.CanvasGroup.interactable = false;
			_inGameMenu.CanvasGroup.blocksRaycasts = false;
		}
	}
}
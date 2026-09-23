using Services;
using UnityEngine;
namespace UI.InGameUIStates
{
	public class InGameMenuState : IInGameUIState
	{
		private readonly IInputService _inputService;
		private readonly InGameMenu _inGameMenu;

		public InGameMenuState(IInputService inputService, InGameMenu inGameMenu)
		{
			_inputService = inputService;
			_inGameMenu = inGameMenu;
		}

		public void Enter()
		{
			_inputService.Disable();
			
			Cursor.lockState = CursorLockMode.None;
			_inGameMenu.CanvasGroup.alpha = 1.0f;
			_inGameMenu.CanvasGroup.interactable = true;
			_inGameMenu.CanvasGroup.blocksRaycasts = true;
		}

		public void Exit()
		{
			_inputService.Enable();
			
			_inGameMenu.CanvasGroup.alpha = 0.0f;
			_inGameMenu.CanvasGroup.interactable = false;
			_inGameMenu.CanvasGroup.blocksRaycasts = false;
		}
	}
}
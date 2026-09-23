using Services;
using UnityEngine;
namespace UI.InGameUIStates
{
	public class ChooseClassMenuState : IInGameUIState
	{
		private readonly ChooseClassMenu _chooseClassMenu;
		private readonly IInputService _inputService;

		public ChooseClassMenuState(IInputService inputService, ChooseClassMenu chooseClassMenu)
		{
			_inputService = inputService;
			_chooseClassMenu = chooseClassMenu;
		}

		public void Enter()
		{
			_inputService.Disable();

			Cursor.lockState = CursorLockMode.None;
			_chooseClassMenu.CanvasGroup.alpha = 1.0f;
			_chooseClassMenu.CanvasGroup.interactable = true;
			_chooseClassMenu.CanvasGroup.blocksRaycasts = true;
		}

		public void Exit()
		{
			_inputService.Enable();

			_chooseClassMenu.CanvasGroup.alpha = 0.0f;
			_chooseClassMenu.CanvasGroup.interactable = false;
			_chooseClassMenu.CanvasGroup.blocksRaycasts = false;
		}
	}
}
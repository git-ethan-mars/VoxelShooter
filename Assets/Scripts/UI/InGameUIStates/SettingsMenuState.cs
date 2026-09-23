using Services;
namespace UI.InGameUIStates
{
	public class SettingsMenuState : IInGameUIState
	{
		private readonly IInputService _inputService;
		private readonly SettingsMenu _settingsMenu;

		public SettingsMenuState(IInputService inputService, SettingsMenu settingsMenu)
		{
			_inputService = inputService;
			_settingsMenu = settingsMenu;
		}

		public void Enter()
		{
			_inputService.Disable();
			
			_settingsMenu.CanvasGroup.alpha = 1;
			_settingsMenu.CanvasGroup.interactable = true;
			_settingsMenu.CanvasGroup.blocksRaycasts = true;
			_settingsMenu.Show();
		}

		public void Exit()
		{
			_inputService.Enable();
			
			_settingsMenu.CanvasGroup.alpha = 0;
			_settingsMenu.CanvasGroup.interactable = false;
			_settingsMenu.CanvasGroup.blocksRaycasts = false;
		}
	}
}
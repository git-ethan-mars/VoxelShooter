using UnityEngine;

namespace UI.SettingsMenuStates
{
	public class ControlsSettingsState : ISettingsMenuState
	{
		private readonly GameObject _controlsSection;
		private readonly KeyBindingsView _keyBindingsView;

		public ControlsSettingsState(GameObject controlsSection, KeyBindingsView keyBindingsView)
		{
			_controlsSection = controlsSection;
			_keyBindingsView = keyBindingsView;
		}

		public void Enter()
		{
			_controlsSection.SetActive(true);
			_keyBindingsView.Show();
		}

		public void Exit()
		{
			_controlsSection.SetActive(false);
		}
	}
}

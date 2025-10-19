using System;
using System.Collections.Generic;
namespace UI.SettingsMenuStates
{
	public class SettingsMenuStateMachine
	{
		private readonly Dictionary<Type, ISettingsMenuState> _states;
		private ISettingsMenuState _currentState;

		public SettingsMenuStateMachine(ISettingsMenuState mouseSettingsState, ISettingsMenuState volumeSettingsState,
			ISettingsMenuState videoSettingsState)
		{
			_states = new Dictionary<Type, ISettingsMenuState>
			{
				[typeof(MouseSettingsState)] = mouseSettingsState,
				[typeof(VolumeSettingsState)] = volumeSettingsState,
				[typeof(VideoSettingsState)] = videoSettingsState
			};
		}

		public void SwitchState<TState>() where TState : ISettingsMenuState
		{
			ISettingsMenuState nextState = _states[typeof(TState)];
			if (nextState == _currentState)
			{
				return;
			}

			_currentState?.Exit();
			_currentState = nextState;
			_currentState.Enter();
		}

		public void Reset()
		{
			_currentState?.Exit();
			_currentState = null;
		}
	}
}
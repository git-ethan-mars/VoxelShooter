using System;
using System.Collections.Generic;

namespace UI.SettingsMenu.States
{
    public class SettingsMenuStateMachine
    {
        private ISettingsMenuState _currentState;
        private readonly Dictionary<Type, ISettingsMenuState> _states;

        public SettingsMenuStateMachine(ISettingsMenuState mouseSettingsState, ISettingsMenuState volumeSettingsState,
            ISettingsMenuState videoSettingsState)
        {
            _states = new()
            {
                [typeof(MouseSettingsState)] = mouseSettingsState,
                [typeof(VolumeSettingsState)] = volumeSettingsState,
                [typeof(VideoSettingsState)] = videoSettingsState
            };
        }

        public void SwitchState<TState>() where TState : ISettingsMenuState
        {
            var nextState = _states[typeof(TState)];
            if (nextState == _currentState)
            {
                return;
            }

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        public void Clear()
        {
            _currentState?.Exit();
        }
    }
}
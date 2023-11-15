using System;
using System.Collections.Generic;

namespace UI.SettingsMenu.States
{
    public class SettingsMenuStateMachine
    {
        public ISettingsMenuState CurrentState { private set; get; }
        private readonly Dictionary<Type, ISettingsMenuState> _states = new()
        {
            [typeof(MouseSettings)] = new MouseSettings(),
            [typeof(VolumeSettings)] = new VolumeSettings(),
            [typeof(VideoSettings)] = new VideoSettings(),
        };

        public void SwitchState<TState>() where TState : ISettingsMenuState
        {
            var nextState = _states[typeof(TState)];
            if (nextState == CurrentState)
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }
    }
}
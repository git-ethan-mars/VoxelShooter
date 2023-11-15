using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class SettingsMenuStateMachine
    {
        public ISettingsMenuState CurrentState { private set; get; }
        private readonly Dictionary<Type, ISettingsMenuState> _states;

        public SettingsMenuStateMachine(GameObject mouseSettings, GameObject volumeSettings, GameObject videoSettings)
        {
            _states = new()
            {
                [typeof(MouseSettings)] = new MouseSettings(mouseSettings),
                [typeof(VolumeSettings)] = new VolumeSettings(volumeSettings),
                [typeof(VideoSettings)] = new VideoSettings(videoSettings),
            };
        }

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
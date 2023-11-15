using System;
using System.Collections.Generic;
using UI.SettingsMenu.States;
using UnityEngine;

namespace UI.SettingsMenu
{
    public class SettingsMenu : MonoBehaviour
    {
        private Dictionary<Type, ISettingsMenuState> _states;
        private ISettingsMenuState _currentState;

        public void Construct()
        {
            _states = new Dictionary<Type, ISettingsMenuState>()
            {
                [typeof(MouseSettings)] = new MouseSettings(),
                [typeof(VolumeSettings)] = new VolumeSettings(),
                [typeof(VideoSettings)] = new VideoSettings(),
            };
        }

        private void Update()
        {
            _currentState.Update();
        }

        private void SwitchState<TState>() where TState : ISettingsMenuState
        {
            _currentState?.Exit();
            _currentState = _states[typeof(TState)];
            _currentState.Enter();
        }
    }
}
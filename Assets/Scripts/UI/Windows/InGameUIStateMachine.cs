using System;
using System.Collections.Generic;
using UI.Windows.States;
using UnityEngine;

namespace UI.Windows
{
    public class InGameUIStateMachine
    {
        private readonly Dictionary<Type, IInGameUIState> _states;
        private IInGameUIState _currentState;

        public InGameUIStateMachine(Dictionary<Type, IInGameUIState> states)
        {
            _states = states;
        }

        public void SwitchState<T>() where T : IInGameUIState
        {
            if (_currentState is T)
            {
                return;
            }


            Debug.Log(typeof(T).ToString());
            _currentState?.Exit();
            _currentState = _states[typeof(T)];
            _currentState.Enter();
        }
    }
}
using System;
using System.Collections.Generic;
using GamePlay.Services;

namespace UI.InGameUIStates
{
	public class InGameUIStateMachine
	{
		private readonly Dictionary<Type, IInGameUIState> _states;
		private IInGameUIState _currentState;

		public InGameUIStateMachine(IInputService inputService, TimeCounter timeCounter,
			ChooseClassMenu chooseClassMenu, InGameMenu inGameMenu, Scoreboard scoreboard)
		{
			_states = new Dictionary<Type, IInGameUIState>
			{
				[typeof(DefaultState)] =
					new DefaultState(inputService, timeCounter),
				[typeof(ChooseClassMenuState)] =
					new ChooseClassMenuState(this, chooseClassMenu),
				[typeof(InGameMenuState)] =
					new InGameMenuState(inGameMenu),
				[typeof(ScoreboardState)] =
					new ScoreboardState(inputService, scoreboard)
			};
		}

		public void SwitchState<T>() where T : IInGameUIState
		{
			if (_currentState is T)
			{
				return;
			}

			_currentState?.Exit();
			_currentState = _states[typeof(T)];
			_currentState.Enter();
		}

		public void Destroy()
		{
			_currentState?.Exit();
		}
	}
}
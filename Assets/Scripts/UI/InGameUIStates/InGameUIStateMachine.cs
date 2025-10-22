using System;
using System.Collections.Generic;
using GamePlay;
using Services;
namespace UI.InGameUIStates
{
	public class InGameUIStateMachine
	{
		private readonly Dictionary<Type, IInGameUIState> _states;
		private IInGameUIState _currentState;

		public InGameUIStateMachine(IInputService inputService, CharacterProvider characterProvider, InGameUI inGameUI)
		{
			_states = new Dictionary<Type, IInGameUIState>
			{
				[typeof(DefaultState)] = new DefaultState(characterProvider, inGameUI.TimeInfo, inGameUI.Hud),
				[typeof(ChooseClassMenuState)] = new ChooseClassMenuState(inputService, inGameUI.ChooseClassMenu),
				[typeof(InGameMenuState)] = new InGameMenuState(inputService, inGameUI.InGameMenu),
				[typeof(ScoreboardState)] = new ScoreboardState(inGameUI.Scoreboard),
				[typeof(SettingsMenuState)] = new SettingsMenuState(inputService, inGameUI.SettingsMenu),
				[typeof(WorldMapState)] = new WorldMapState(inGameUI.WorldMap)
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
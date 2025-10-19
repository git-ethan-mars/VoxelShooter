using System;
using System.Collections.Generic;
namespace Infrastructure.States
{
	public class GameStateMachine
	{
		private readonly Dictionary<Type, IExitableState> _registeredStates = new Dictionary<Type, IExitableState>();
		private IExitableState _activeState;

		public void RegisterState<TState>(TState state) where TState : class, IExitableState
		{
			if (_registeredStates.ContainsKey(typeof(TState)))
			{
				_registeredStates[typeof(TState)] = state;
			}
			else
			{
				_registeredStates.Add(typeof(TState), state);
			}
		}

		public void Enter<TState>() where TState : class, IState
		{
			var state = ChangeState<TState>();
			state.Enter();
		}

		public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
		{
			var state = ChangeState<TState>();
			state.Enter(payload);
		}

		private TState ChangeState<TState>() where TState : class, IExitableState
		{
			_activeState?.Exit();
			var state = GetState<TState>();
			_activeState = state;
			return state;
		}

		private TState GetState<TState>() where TState : class, IExitableState
		{
			return _registeredStates[typeof(TState)] as TState;
		}
	}
}
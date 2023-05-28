using System;
using System.Collections.Generic;
using Data;
using Infrastructure.Services.StaticData;

namespace PlayerLogic.States
{
    public class PlayerStateMachine
    {
        private readonly Dictionary<Type, IPlayerState> _states;
        private IPlayerState _currentState;

        public PlayerStateMachine(PlayerData playerData, IStaticDataService staticData)
        {
            _states = new Dictionary<Type, IPlayerState>()
            {
                [typeof(LifeState)] = new LifeState(playerData, staticData),
                [typeof(DeathState)] = new DeathState(playerData),
            };
        }


        public void Enter<TState>() where TState : IPlayerState
        {
            _currentState?.Exit();
            _currentState = _states[typeof(TState)];
            _currentState.Enter();
        }
        
    }
}
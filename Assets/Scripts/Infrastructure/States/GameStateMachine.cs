using System;
using System.Collections.Generic;
using Infrastructure.Factory;
using Infrastructure.Services;

namespace Infrastructure.States
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;

        public GameStateMachine(SceneLoader sceneLoader, ICoroutineRunner coroutineRunner, AllServices allServices,
            bool isLocalBuild)
        {
            _states = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader, allServices, coroutineRunner, isLocalBuild),
                [typeof(MainMenuState)] = new MainMenuState(this, sceneLoader, allServices.Single<IUIFactory>(), isLocalBuild),
                [typeof(CreateMatchState)] = new CreateMatchState(this, allServices.Single<IUIFactory>(), isLocalBuild),
                [typeof(StartSteamLobbyState)] = new StartSteamLobbyState(this, sceneLoader, allServices.Single<IGameFactory>()),
                [typeof(JoinSteamLobbyState)] = new JoinSteamLobbyState(this, sceneLoader, allServices.Single<IGameFactory>()),
                [typeof(StartMatchState)] = new StartMatchState(this, sceneLoader, allServices.Single<IGameFactory>()),
                [typeof(JoinLocalMatchState)] = new JoinLocalMatchState(this,  sceneLoader, allServices.Single<IGameFactory>()),
                [typeof(GameLoopState)] =
                    new GameLoopState(allServices.Single<IUIFactory>(), isLocalBuild)
            };
            
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
            TState state = GetState<TState>();
            _activeState = state;
            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState
        {
            return _states[typeof(TState)] as TState;
        }
    }
}
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
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader, allServices, coroutineRunner),
                [typeof(MainMenuState)] = new MainMenuState(this, sceneLoader, allServices.Single<IUIFactory>()),
                [typeof(CreateMatchState)] = new CreateMatchState(this, allServices.Single<IUIFactory>()),
                [typeof(StartMatchState)] = new StartMatchState(this, sceneLoader, allServices.Single<IGameFactory>(), isLocalBuild),
                [typeof(JoinMatchState)] = new JoinMatchState(this,  sceneLoader, allServices.Single<IGameFactory>(),isLocalBuild),
                [typeof(GameLoopState)] =
                    new GameLoopState(allServices.Single<IUIFactory>())
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

        public void Enter<TState, TPayload1, TPayload2>(TPayload1 payload1, TPayload2 payload2)
            where TState : class, IPayloadedState<TPayload1, TPayload2>
        {
            var state = ChangeState<TState>();
            state.Enter(payload1, payload2);
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
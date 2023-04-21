using System;
using System.Collections.Generic;
using Infrastructure.AssetManagement;
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
                [typeof(LoadMapState)] = new LoadMapState(this, sceneLoader, allServices.Single<IGameFactory>(),
                    allServices.Single<IStaticDataService>(), allServices.Single<IAssetProvider>(), allServices.Single<IParticleFactory>(), isLocalBuild),
                [typeof(GameLoopState)] =
                    new GameLoopState(allServices.Single<IGameFactory>())
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
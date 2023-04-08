using Infrastructure.Factory;
using Networking;

namespace Infrastructure.States
{
    public class LoadMapState : IPayloadedState<string>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;

        public LoadMapState(GameStateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory)
        {
            _sceneLoader = sceneLoader;
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
        }

        public void Enter(string sceneName)
        {
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        private void OnLoaded()
        {
            var networkManager = _gameFactory.CreateNetworkManager();
            networkManager.GetComponent<CustomNetworkManager>().ConnectionHappened +=
                () => _stateMachine.Enter<GameLoopState>();
        }

        public void Exit()
        {
        }
    }
}
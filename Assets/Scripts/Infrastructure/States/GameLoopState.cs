using System.Collections;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;

namespace Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IGameFactory _gameFactory;
        private readonly IMapGeneratorProvider _mapGeneratorProvider;


        public GameLoopState(ICoroutineRunner coroutineRunner, IGameFactory gameFactory)
        {
            _coroutineRunner = coroutineRunner;
            _gameFactory = gameFactory;
        }

        public void Enter()
        {
            _coroutineRunner.StartCoroutine(WaitForLocalPlayer());
        }

        public void Exit()
        {
        }

        private IEnumerator WaitForLocalPlayer()
        {
            while (!NetworkClient.localPlayer)
            {
                yield return null;
            }

            var player = _gameFactory.InitializeLocalPlayer();
            _gameFactory.CreateHud(player);

        }
    }
}
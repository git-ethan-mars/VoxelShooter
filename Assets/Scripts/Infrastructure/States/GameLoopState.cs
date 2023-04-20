using Infrastructure.Factory;

namespace Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IGameFactory _gameFactory;

        public GameLoopState(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }
        public void Enter()
        {
            _gameFactory.CreateChangeClassMenu();
        }

        public void Exit()
        {
        }

    }
}
using Infrastructure.Factory;
using Logic;

namespace Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IGameFactory _gameFactory;
        private readonly LoadingCurtain _curtain;


        public GameLoopState(IGameFactory gameFactory, LoadingCurtain loadingCurtain)
        {
            _gameFactory = gameFactory;
            _curtain = loadingCurtain;
        }

        public void Enter()
        {
            _curtain.Show();
            _gameFactory.CreateMapGenerator();
            _curtain.Hide();
        }

        public void Exit()
        {
        }
    }
}
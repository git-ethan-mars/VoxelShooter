using Infrastructure.Factory;

namespace Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IUIFactory _uiFactory;

        public GameLoopState(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }
        public void Enter()
        {
            _uiFactory.CreateChangeClassMenu();
        }

        public void Exit()
        {
        }

    }
}
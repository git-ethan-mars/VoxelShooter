using Infrastructure.Factory;

namespace Infrastructure.States
{
    public class CreateMatchState : IState
    {
        private readonly IUIFactory _uiFactory;
        private readonly GameStateMachine _stateMachine;

        public CreateMatchState(GameStateMachine stateMachine, IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
        }
        public void Enter()
        {
            _uiFactory.CreateMatchMenu(_stateMachine);
        }

        public void Exit()
        {
        }
    }
}
using Infrastructure.Factory;

namespace Infrastructure.States
{
    public class CreateMatchState : IState
    {
        private readonly IUIFactory _uiFactory;
        private readonly GameStateMachine _stateMachine;
        private readonly bool _isLocalBuild;

        public CreateMatchState(GameStateMachine stateMachine, IUIFactory uiFactory, bool isLocalBuild)
        {
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
            _isLocalBuild = isLocalBuild;
        }
        public void Enter()
        {
            _uiFactory.CreateMatchMenu(_stateMachine, _isLocalBuild);
        }

        public void Exit()
        {
        }
    }
}
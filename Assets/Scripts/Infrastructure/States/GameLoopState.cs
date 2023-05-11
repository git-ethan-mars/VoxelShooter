using Infrastructure.Factory;

namespace Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IUIFactory _uiFactory;
        private readonly bool _isLocalBuild;

        public GameLoopState(IUIFactory uiFactory, bool isLocalBuild)
        {
            _uiFactory = uiFactory;
            _isLocalBuild = isLocalBuild;
        }
        public void Enter()
        {
            _uiFactory.CreateChooseClassMenu(_isLocalBuild);
        }

        public void Exit()
        {
        }

    }
}
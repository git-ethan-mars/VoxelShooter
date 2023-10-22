using Infrastructure.Factory;
using Networking;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<IClient>
    {
        private readonly IUIFactory _uiFactory;
        private readonly bool _isLocalBuild;

        public GameLoopState(IUIFactory uiFactory, bool isLocalBuild)
        {
            _uiFactory = uiFactory;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter(IClient client)
        {
            _uiFactory.CreateChooseClassMenu(client, _isLocalBuild);
            _uiFactory.CreateTimeCounter(client);
            _uiFactory.CreateScoreboard(client);
        }

        public void Exit()
        {
        }
    }
}
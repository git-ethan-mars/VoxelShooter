using Infrastructure.Factory;
using Networking;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<CustomNetworkManager>
    {
        private readonly IUIFactory _uiFactory;
        private readonly bool _isLocalBuild;

        public GameLoopState(IUIFactory uiFactory, bool isLocalBuild)
        {
            _uiFactory = uiFactory;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter(CustomNetworkManager networkManager)
        {
            _uiFactory.CreateChooseClassMenu(_isLocalBuild);
            _uiFactory.CreateTimeCounter(networkManager);
            _uiFactory.CreateScoreboard(networkManager);
        }

        public void Exit()
        {
        }

    }
}
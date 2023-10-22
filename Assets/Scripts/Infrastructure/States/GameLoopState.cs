using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Networking;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<IClient>
    {
        private readonly IUIFactory _uiFactory;
        private readonly IInputService _inputService;
        private readonly IAvatarLoader _avatarLoader;
        private readonly bool _isLocalBuild;

        public GameLoopState(IUIFactory uiFactory, IInputService inputService, IAvatarLoader avatarLoader,
            bool isLocalBuild)
        {
            _uiFactory = uiFactory;
            _inputService = inputService;
            _avatarLoader = avatarLoader;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter(IClient client)
        {
            _uiFactory.CreateChooseClassMenu(client, _inputService, _isLocalBuild);
            _uiFactory.CreateTimeCounter(client, _inputService);
            _uiFactory.CreateScoreboard(client, _inputService, _avatarLoader);
        }

        public void Exit()
        {
        }
    }
}
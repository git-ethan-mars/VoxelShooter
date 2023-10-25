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

        public GameLoopState(IUIFactory uiFactory, IInputService inputService, IAvatarLoader avatarLoader)
        {
            _uiFactory = uiFactory;
            _inputService = inputService;
            _avatarLoader = avatarLoader;
        }

        public void Enter(IClient client)
        {
            _uiFactory.CreateChooseClassMenu(client, _inputService);
            _uiFactory.CreateTimeCounter(client, _inputService);
            _uiFactory.CreateScoreboard(client, _inputService, _avatarLoader);
        }

        public void Exit()
        {
        }
    }
}
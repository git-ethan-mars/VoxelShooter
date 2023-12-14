using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.Storage;
using Mirror;
using Networking;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<CustomNetworkManager>
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IUIFactory _uiFactory;
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;
        private readonly IAvatarLoader _avatarLoader;

        public GameLoopState(GameStateMachine gameStateMachine, IUIFactory uiFactory, IInputService inputService,
            IStorageService storageService,
            IAvatarLoader avatarLoader)
        {
            _gameStateMachine = gameStateMachine;
            _uiFactory = uiFactory;
            _inputService = inputService;
            _storageService = storageService;
            _avatarLoader = avatarLoader;
        }

        public void Enter(CustomNetworkManager networkManager)
        {
            _uiFactory.CreateInGameUI(_gameStateMachine, networkManager, _inputService, _storageService, _avatarLoader);
        }

        public void Exit()
        {
        }
    }
}
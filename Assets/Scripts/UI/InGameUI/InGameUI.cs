using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Networking;
using UI.InGameUI.States;
using UnityEngine;

namespace UI.InGameUI
{
    public class InGameUI : MonoBehaviour
    {
        private InGameUIStateMachine _uiStateMachine;

        private IInputService _inputService;

        public void Construct(GameStateMachine gameStateMachine, CustomNetworkManager networkManager,
            IUIFactory uiFactory, IStorageService storageService,
            IInputService inputService,
            IAvatarLoader avatarLoader)
        {
            _inputService = inputService;
            _uiStateMachine = new InGameUIStateMachine(gameStateMachine, networkManager, inputService, uiFactory,
                storageService,
                avatarLoader, transform);
            _uiStateMachine.SwitchState<ChooseClassMenuState>();
        }

        private void Update()
        {
            if (_inputService.IsScoreboardButtonUp())
            {
                _uiStateMachine.SwitchState<DefaultState>();
            }

            if (_inputService.IsScoreboardButtonDown())
            {
                _uiStateMachine.SwitchState<ScoreboardState>();
            }

            if (_inputService.IsChooseClassButtonDown())
            {
                _uiStateMachine.SwitchState<ChooseClassMenuState>();
            }

            if (_inputService.IsInGameMenuButtonDown())
            {
                _uiStateMachine.SwitchState<InGameMenuState>();
            }
        }

        private void OnDestroy()
        {
            _uiStateMachine.Destroy();
        }
    }
}
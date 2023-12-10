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
        private const float ShowResultsDuration = 10;

        private GameStateMachine _gameStateMachine;
        private IInputService _inputService;
        private InGameUIStateMachine _uiStateMachine;
        private CustomNetworkManager _networkManager;

        public void Construct(GameStateMachine gameStateMachine, CustomNetworkManager networkManager,
            IUIFactory uiFactory, IStorageService storageService,
            IInputService inputService,
            IAvatarLoader avatarLoader)
        {
            _gameStateMachine = gameStateMachine;
            _inputService = inputService;
            _uiStateMachine = new InGameUIStateMachine(gameStateMachine, networkManager, inputService, uiFactory,
                storageService,
                avatarLoader, transform);
            _uiStateMachine.SwitchState<ChooseClassMenuState>();
            _networkManager = networkManager;
            _networkManager.Client.GameFinished += OnGameFinished;
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

        private void OnGameFinished()
        {
            enabled = false;
            _uiStateMachine.SwitchState<ScoreboardState>();
            _networkManager.StartCoroutine(Utils.DoActionAfterDelay(_gameStateMachine.Enter<MainMenuState>,
                ShowResultsDuration));
        }

        private void OnDestroy()
        {
            _uiStateMachine.Destroy();
            _networkManager.Client.GameFinished -= OnGameFinished;
        }
    }
}
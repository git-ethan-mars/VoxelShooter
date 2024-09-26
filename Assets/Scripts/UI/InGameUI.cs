using Common.Input;
using Common.Storage;
using Infrastructure.Factory;
using UI.InGameUIStates;
using UnityEngine;

namespace UI
{
    public class InGameUI : MonoBehaviour
    {
        public Scoreboard Scoreboard { get; private set; }
        public TimeCounter TimeCounter { get; private set; }
        public ChooseClassMenu ChooseClassMenu { get; private set; }
        public SettingsMenu SettingsMenu { get; private set; }
        public InGameMenu InGameMenu { get; private set; }
        private IInputService _inputService;
        private InGameUIStateMachine _uiStateMachine;

        public void Construct(IUIFactory uiFactory, IStorageService storageService,
            IInputService inputService,
            IAvatarLoader avatarLoader)
        {
            _inputService = inputService;
            TimeCounter = uiFactory.CreateTimeCounter(transform);
            ChooseClassMenu = uiFactory.CreateChooseClassMenu(transform);
            InGameMenu = uiFactory.CreateInGameMenu(transform);
            SettingsMenu = uiFactory.CreateSettingsMenu(storageService, transform);
            Scoreboard = uiFactory.CreateScoreBoard(avatarLoader, transform);
            _uiStateMachine = new InGameUIStateMachine(inputService,
                TimeCounter, ChooseClassMenu, InGameMenu, Scoreboard);
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

        public void ShowFinalStatistic()
        {
            enabled = false;
            _uiStateMachine.SwitchState<ScoreboardState>();
        }

        private void OnDestroy()
        {
            _uiStateMachine.Destroy();
        }
    }
}
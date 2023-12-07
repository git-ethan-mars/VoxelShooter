using System;
using System.Collections.Generic;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Networking;
using UI.Windows.States;
using UnityEngine;

namespace UI.Windows
{
    public class InGameUI : MonoBehaviour
    {
        private InGameUIStateMachine _uiStateMachine;
        private IInputService _inputService;
        private ChooseClassMenu _chooseClassMenu;

        public void Construct(IUIFactory uiFactory, IInputService inputService, IClient client,
            IAvatarLoader avatarLoader)
        {
            _inputService = inputService;
            _chooseClassMenu = uiFactory.CreateChooseClassMenu(transform);
            var timeCounter = uiFactory.CreateTimeCounter(transform, client);
            var scoreboard = uiFactory.CreateScoreBoard(transform, client, avatarLoader);
            var inGameMenu = uiFactory.CreateInGameMenu(transform);
            _uiStateMachine = new InGameUIStateMachine(new Dictionary<Type, IInGameUIState>
            {
                [typeof(DefaultState)] = new DefaultState(timeCounter),
                [typeof(ChooseClassMenuState)] = new ChooseClassMenuState(_chooseClassMenu),
                [typeof(InGameMenuState)] = new InGameMenuState(inGameMenu),
                [typeof(ScoreboardState)] = new ScoreboardState(scoreboard)
            });
            _uiStateMachine.SwitchState<ChooseClassMenuState>();
            _chooseClassMenu.BuilderButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.SniperButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.CombatantButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.GrenadierButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.ExitButton.onClick.AddListener(_uiStateMachine.SwitchState<DefaultState>);
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
            _chooseClassMenu.BuilderButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.SniperButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.CombatantButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.GrenadierButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
            _chooseClassMenu.ExitButton.onClick.RemoveListener(_uiStateMachine.SwitchState<DefaultState>);
        }
    }
}
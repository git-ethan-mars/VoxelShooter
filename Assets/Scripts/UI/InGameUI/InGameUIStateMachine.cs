using System;
using System.Collections.Generic;
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
    public class InGameUIStateMachine
    {
        private readonly Dictionary<Type, IInGameUIState> _states;
        private IInGameUIState _currentState;

        public InGameUIStateMachine(GameStateMachine gameStateMachine, CustomNetworkManager networkManager,
            IInputService inputService,
            IUIFactory uiFactory, IStorageService storageService,
            IAvatarLoader avatarLoader, Transform parent)
        {
            _states = new Dictionary<Type, IInGameUIState>
            {
                [typeof(DefaultState)] =
                    new DefaultState(networkManager, inputService, uiFactory.CreateTimeCounter(parent, networkManager)),
                [typeof(ChooseClassMenuState)] =
                    new ChooseClassMenuState(this, uiFactory.CreateChooseClassMenu(parent)),
                [typeof(InGameMenuState)] =
                    new InGameMenuState(gameStateMachine, this, uiFactory, storageService, networkManager,
                        uiFactory.CreateInGameMenu(parent)),
                [typeof(ScoreboardState)] =
                    new ScoreboardState(inputService, uiFactory.CreateScoreBoard(parent, networkManager, avatarLoader))
            };
        }

        public void SwitchState<T>() where T : IInGameUIState
        {
            if (_currentState is T)
            {
                return;
            }

            _currentState?.Exit();
            _currentState = _states[typeof(T)];
            _currentState.Enter();
        }

        public void Destroy()
        {
            _currentState?.Exit();
        }
    }
}
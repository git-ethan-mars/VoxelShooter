using System;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Networking;
using PlayerLogic;
using UI;
using UI.InGameUI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        Hud CreateHud(IClient client, Player player, IInputService inputService);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine);
        GameObject CreateMatchMenu(GameStateMachine gameStateMachine, IMapRepository mapRepository);
        GameObject CreateSettingsMenu(IStorageService storageService, Action onBackButtonPressed);
        void CreateLoadingWindow(CustomNetworkManager networkManager);

        void CreateInGameUI(GameStateMachine gameStateMachine, CustomNetworkManager networkManager,
            IInputService inputService, IStorageService storageService,
            IAvatarLoader avatarLoader);

        ChooseClassMenu CreateChooseClassMenu(Transform parent);
        Scoreboard CreateScoreBoard(Transform parent, CustomNetworkManager networkManager, IAvatarLoader avatarLoader);
        TimeCounter CreateTimeCounter(Transform parent, CustomNetworkManager networkManager);
        InGameMenu CreateInGameMenu(Transform parent);
    }
}
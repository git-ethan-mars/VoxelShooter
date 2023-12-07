using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Networking;
using PlayerLogic;
using UI;
using UI.Windows;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        Hud CreateHud(Player player, IInputService inputService);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine);
        GameObject CreateMatchMenu(GameStateMachine gameStateMachine, IMapRepository mapRepository);
        GameObject CreateSettingsMenu(GameStateMachine gameStateMachine, IStorageService storageService);
        void CreateLoadingWindow(IClient client);
        void CreateInGameUI(IClient client, IInputService inputService, IAvatarLoader avatarLoader);
        ChooseClassMenu CreateChooseClassMenu(Transform parent);
        Scoreboard CreateScoreBoard(Transform parent, IClient client, IAvatarLoader avatarLoader);
        TimeCounter CreateTimeCounter(Transform parent, IClient client);
        InGameMenu CreateInGameMenu(Transform parent);
    }
}
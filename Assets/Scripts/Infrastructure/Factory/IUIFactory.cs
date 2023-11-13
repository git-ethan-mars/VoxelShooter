using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Networking;
using PlayerLogic;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        Hud CreateHud(Player player, IClient client, IInputService inputService);
        void CreateChooseClassMenu(IClient client, IInputService inputService);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine);
        GameObject CreateMatchMenu(IMapRepository mapRepository, GameStateMachine gameStateMachine);
        void CreateTimeCounter(IClient client, IInputService inputService);
        void CreateScoreboard(IClient client, IInputService inputService, IAvatarLoader avatarLoader);
        void CreateLoadingWindow(IClient client);
    }
}
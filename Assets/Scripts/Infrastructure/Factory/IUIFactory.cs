using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        GameObject CreateHud(IClient client, IInputService inputService, GameObject player);
        void CreateChooseClassMenu(IClient client, IInputService inputService, bool isLocalBuild);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
        GameObject CreateMatchMenu(IMapRepository mapRepository, GameStateMachine gameStateMachine, bool isLocalBuild);
        void CreateTimeCounter(IClient client, IInputService inputService);
        void CreateScoreboard(IClient client, IInputService inputService, IAvatarLoader avatarLoader);
        void CreateLoadingWindow(IClient client);
    }
}
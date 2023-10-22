using Infrastructure.Services;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        GameObject CreateHud(IClient client, GameObject player);
        void CreateChooseClassMenu(IClient client, bool isLocalBuild);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
        GameObject CreateMatchMenu(IMapRepository mapRepository, GameStateMachine gameStateMachine, bool isLocalBuild);
        void CreateTimeCounter(IClient client);
        void CreateScoreboard(IClient client);
        void CreateLoadingWindow(IClient client);
    }   
}
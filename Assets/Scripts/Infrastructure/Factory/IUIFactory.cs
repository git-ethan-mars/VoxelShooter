using Infrastructure.Services;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        GameObject CreateHud(GameObject player);
        void CreateChooseClassMenu(CustomNetworkManager networkManager, bool isLocalBuild);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
        GameObject CreateMatchMenu(IMapRepository mapRepository, GameStateMachine gameStateMachine, bool isLocalBuild);
        void CreateTimeCounter(CustomNetworkManager networkManager);
        void CreateScoreboard(CustomNetworkManager networkManager);
        void CreateLoadingWindow(CustomNetworkManager networkManager);
    }   
}
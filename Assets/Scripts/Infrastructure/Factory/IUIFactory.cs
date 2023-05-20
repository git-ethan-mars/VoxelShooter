using Infrastructure.Services;
using Infrastructure.States;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        GameObject CreateHud(GameObject player);
        void CreateChooseClassMenu(bool isLocalBuild);
        GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
        void CreateMatchMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
        void CreateTimeCounter(CustomNetworkManager networkManager);
        void CreateScoreboard(CustomNetworkManager networkManager);
    }   
}
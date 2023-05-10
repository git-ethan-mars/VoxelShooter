using Infrastructure.Services;
using Infrastructure.States;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IUIFactory : IService
    {
        GameObject CreateHud(GameObject player);
        void CreateChangeClassMenu();
        GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
        void CreateMatchMenu(GameStateMachine gameStateMachine, bool isLocalBuild);
    }   
}
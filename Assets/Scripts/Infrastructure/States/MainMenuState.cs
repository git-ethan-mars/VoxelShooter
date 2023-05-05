using Infrastructure.Factory;
using UnityEngine;

namespace Infrastructure.States
{
    public class MainMenuState : IPayloadedState<string>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly IUIFactory _uiFactory;
        private readonly GameStateMachine _stateMachine;
        private GameObject _mainMenu;

        public MainMenuState(GameStateMachine stateMachine,SceneLoader sceneLoader, IUIFactory uiFactory)
        {
            _sceneLoader = sceneLoader;
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
        }
        public void Enter(string sceneName)
        {
            _sceneLoader.Load(sceneName, EnterLoadLevel);
        }

        private void EnterLoadLevel()
        { 
            _mainMenu = _uiFactory.CreateMainMenu(_stateMachine);
        }

        public void Exit()
        {
            Object.Destroy(_mainMenu);
        }
    }
}
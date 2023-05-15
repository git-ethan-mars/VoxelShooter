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
        private readonly bool _isLocalBuild;

        public MainMenuState(GameStateMachine stateMachine, SceneLoader sceneLoader, IUIFactory uiFactory,
            bool isLocalBuild)
        {
            _sceneLoader = sceneLoader;
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter(string sceneName)
        {
            _sceneLoader.Load(sceneName, EnterLoadLevel);
        }

        private void EnterLoadLevel()
        {
            _mainMenu = _uiFactory.CreateMainMenu(_stateMachine, _isLocalBuild);
        }

        public void Exit()
        {
            Object.Destroy(_mainMenu);
        }
    }
}
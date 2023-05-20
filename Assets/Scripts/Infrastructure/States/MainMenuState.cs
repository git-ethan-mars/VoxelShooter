using Infrastructure.Factory;
using UnityEngine;

namespace Infrastructure.States
{
    public class MainMenuState : IState
    {
        private readonly SceneLoader _sceneLoader;
        private readonly IUIFactory _uiFactory;
        private readonly GameStateMachine _stateMachine;
        private GameObject _mainMenu;
        private readonly bool _isLocalBuild;
        private const string MainMenu = "MainMenu";
        
        public MainMenuState(GameStateMachine stateMachine, SceneLoader sceneLoader, IUIFactory uiFactory,
            bool isLocalBuild)
        {
            _sceneLoader = sceneLoader;
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter()
        {
            _sceneLoader.Load(MainMenu, EnterLoadLevel);
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
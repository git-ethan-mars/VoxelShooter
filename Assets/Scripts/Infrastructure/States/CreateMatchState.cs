using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using UnityEngine;

namespace Infrastructure.States
{
    public class CreateMatchState : IState
    {
        private readonly IUIFactory _uiFactory;
        private readonly GameStateMachine _stateMachine;
        private readonly IMapRepository _mapRepository;
        private readonly bool _isLocalBuild;
        private GameObject _matchMenu;

        public CreateMatchState(GameStateMachine stateMachine, IMapRepository mapRepository, IUIFactory uiFactory,
            bool isLocalBuild)
        {
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
            _mapRepository = mapRepository;
            _isLocalBuild = isLocalBuild;
        }

        public void Enter()
        {
            _matchMenu = _uiFactory.CreateMatchMenu(_mapRepository, _stateMachine, _isLocalBuild);
        }

        public void Exit()
        {
            Object.Destroy(_matchMenu);
        }
    }
}
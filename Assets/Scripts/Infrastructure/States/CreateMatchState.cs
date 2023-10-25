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
        private GameObject _matchMenu;

        public CreateMatchState(GameStateMachine stateMachine, IMapRepository mapRepository, IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
            _mapRepository = mapRepository;
        }

        public void Enter()
        {
            _matchMenu = _uiFactory.CreateMatchMenu(_mapRepository, _stateMachine);
        }

        public void Exit()
        {
            Object.Destroy(_matchMenu);
        }
    }
}
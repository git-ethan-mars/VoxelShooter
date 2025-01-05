using GamePlay.Data;
using UI;
using UnityEngine;
using VoxelMap;

namespace Infrastructure.States
{
    public class CreateMatchState : IState
    {
        private readonly IUIFactory _uiFactory;
        private readonly GameStateMachine _stateMachine;
        private readonly IMapRepository _mapRepository;
        private MatchMenu _matchMenu;

        public CreateMatchState(GameStateMachine stateMachine, IMapRepository mapRepository, IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
            _stateMachine = stateMachine;
            _mapRepository = mapRepository;
        }

        public void Enter()
        {
            _matchMenu = _uiFactory.CreateMatchMenu(_mapRepository);
            _matchMenu.BackButtonPressed += OnBackButton;
            _matchMenu.ApplyButtonPressed += OnApplyButton;
        }
        
        private void OnBackButton()
        {
            _stateMachine.Enter<MainMenuState>();
        }
        
        private void OnApplyButton(WorldSettings worldSettings)
        {
# if LOCAL_BUILD
            _stateMachine.Enter<StartMatchState, WorldSettings>(
                worldSettings);
# else
            _stateMachine.Enter<StartSteamLobbyState, WorldSettings>(
                worldSettings);
# endif
        }

        public void Exit()
        {
            _matchMenu.BackButtonPressed -= OnBackButton;
            _matchMenu.ApplyButtonPressed -= OnApplyButton;
            Object.Destroy(_matchMenu.gameObject);
        }
    }
}
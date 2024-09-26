using Common.Services.StaticData;
using Common.StaticData;
using Infrastructure.Factory;
using UI;
using UnityEngine;

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
        
        private void OnApplyButton()
        {
            var serverSettings = new WorldSettings(mapName.text, _timeLimitation.CurrentValue.Value,
                _lobbyBalance.spawnTime, _lobbyBalance.boxSpawnTime);
# if LOCAL_BUILD
            _stateMachine.Enter<StartMatchState, WorldSettings>(
                serverSettings);
# else
            _stateMachine.Enter<StartSteamLobbyState, WorldSettings>(
                serverSettings);
# endif
        }

        public void Exit()
        {
            Object.Destroy(_matchMenu.gameObject);
        }
    }
}
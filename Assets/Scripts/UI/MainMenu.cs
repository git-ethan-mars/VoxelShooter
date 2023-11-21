using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : Window
    {
        [SerializeField]
        private Button joinMatchButton;

        [SerializeField]
        private Button createMatchButton;

        [SerializeField]
        private Button settingButton;

        [SerializeField]
        private Button exitButton;

        private GameStateMachine _stateMachine;

        public void Construct(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            createMatchButton.onClick.AddListener(stateMachine.Enter<CreateMatchState>);
            settingButton.onClick.AddListener(stateMachine.Enter<SettingsMenuState>);
            if (Constants.isLocalBuild)
            {
                joinMatchButton.onClick.AddListener(stateMachine.Enter<JoinLocalMatchState>);
            }
            else
            {
                joinMatchButton.onClick.AddListener(stateMachine.Enter<JoinSteamLobbyState>);
            }

            exitButton.onClick.AddListener(Application.Quit);
            ShowCursor();
        }

        private void OnDestroy()
        {
            createMatchButton.onClick.RemoveListener(_stateMachine.Enter<CreateMatchState>);
            if (Constants.isLocalBuild)
            {
                joinMatchButton.onClick.RemoveListener(_stateMachine.Enter<JoinLocalMatchState>);
            }
            else
            {
                joinMatchButton.onClick.RemoveListener(_stateMachine.Enter<JoinSteamLobbyState>);
            }

            exitButton.onClick.RemoveListener(Application.Quit);
        }
    }
}
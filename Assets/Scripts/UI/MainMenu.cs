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
        private Button exitButton;

        public void Construct(GameStateMachine stateMachine)
        {
            createMatchButton.onClick.AddListener(stateMachine.Enter<CreateMatchState>);
            if (Constants.isLocalBuild)
            {
                joinMatchButton.onClick.AddListener(stateMachine.Enter<JoinLocalMatchState>);
            }
            else
            {
                joinMatchButton.onClick.AddListener(stateMachine.Enter<JoinSteamLobbyState>);
            }

            exitButton.onClick.AddListener(Application.Quit);
        }

        private void OnEnable()
        {
            ShowCursor();
        }

        private void OnDisable()
        {
            HideCursor();
        }
    }
}
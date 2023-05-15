using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button joinMatchButton;
        [SerializeField] private Button createMatchButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        private const string Main = "Main";

        public void Construct(GameStateMachine stateMachine, bool isLocalBuild)
        {
            createMatchButton.onClick.AddListener(stateMachine.Enter<CreateMatchState>);
            if (isLocalBuild)
                joinMatchButton.onClick.AddListener(() => stateMachine.Enter<JoinLocalMatchState, string>(Main));
            else
                joinMatchButton.onClick.AddListener(() => stateMachine.Enter<JoinSteamLobbyState, string>(Main));
            exitButton.onClick.AddListener(Application.Quit);
        }
    }
}
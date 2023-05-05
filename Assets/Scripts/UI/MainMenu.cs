using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Device.Application;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button joinMatchButton;
        [SerializeField] private Button createMatchButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        private const string Main = "Main";

        public void Construct(GameStateMachine stateMachine)
        {
            createMatchButton.onClick.AddListener(stateMachine.Enter<CreateMatchState>);
            joinMatchButton.onClick.AddListener(() => stateMachine.Enter<JoinMatchState, string>(Main));
            exitButton.onClick.AddListener(Application.Quit);
        }
    }
}
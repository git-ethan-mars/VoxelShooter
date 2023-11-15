using Infrastructure.States;
using UI.SettingsMenu.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SettingsMenu
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField]
        private Button mouseSection;

        [SerializeField]
        private GameObject mouseSettings;

        [SerializeField]
        private Button volumeSection;

        [SerializeField]
        private GameObject volumeSettings;

        [SerializeField]
        private Button videoSection;

        [SerializeField]
        private GameObject videoSettings;

        [SerializeField]
        private Button backButton;


        private GameStateMachine _gameStateMachine;
        private SettingsMenuStateMachine _menuStateMachine;

        public void Construct(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _menuStateMachine = new SettingsMenuStateMachine(mouseSettings, volumeSettings, videoSettings);
            mouseSection.onClick.AddListener(_menuStateMachine.SwitchState<MouseSettingsState>);
            volumeSection.onClick.AddListener(_menuStateMachine.SwitchState<VolumeSettingsState>);
            videoSection.onClick.AddListener(_menuStateMachine.SwitchState<VideoSettingsState>);
            backButton.onClick.AddListener(_gameStateMachine.Enter<MainMenuState>);
            _menuStateMachine.SwitchState<MouseSettingsState>();
        }

        private void Update()
        {
            _menuStateMachine.CurrentState.Update();
        }

        private void OnDestroy()
        {
            mouseSection.onClick.RemoveListener(_menuStateMachine.SwitchState<MouseSettingsState>);
            volumeSection.onClick.RemoveListener(_menuStateMachine.SwitchState<VolumeSettingsState>);
            videoSection.onClick.RemoveListener(_menuStateMachine.SwitchState<VideoSettingsState>);
            backButton.onClick.RemoveListener(_gameStateMachine.Enter<MainMenuState>);
        }
    }
}
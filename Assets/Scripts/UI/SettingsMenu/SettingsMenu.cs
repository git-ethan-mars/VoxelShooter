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
        private Button volumeSection;

        [SerializeField]
        private Button videoSection;

        [SerializeField]
        private Button backButton;

        private GameStateMachine _gameStateMachine;
        private SettingsMenuStateMachine _menuStateMachine;

        public void Construct(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _menuStateMachine = new SettingsMenuStateMachine();
            mouseSection.onClick.AddListener(_menuStateMachine.SwitchState<MouseSettings>);
            volumeSection.onClick.AddListener(_menuStateMachine.SwitchState<VolumeSettings>);
            videoSection.onClick.AddListener(_menuStateMachine.SwitchState<VideoSettings>);
            backButton.onClick.AddListener(_gameStateMachine.Enter<MainMenuState>);
            _menuStateMachine.SwitchState<MouseSettings>();
        }

        private void Update()
        {
            _menuStateMachine.CurrentState.Update();
        }

        private void OnDestroy()
        {
            mouseSection.onClick.RemoveListener(_menuStateMachine.SwitchState<MouseSettings>);
            volumeSection.onClick.RemoveListener(_menuStateMachine.SwitchState<VolumeSettings>);
            videoSection.onClick.RemoveListener(_menuStateMachine.SwitchState<VideoSettings>);
            backButton.onClick.RemoveListener(_gameStateMachine.Enter<MainMenuState>);
        }
    }
}
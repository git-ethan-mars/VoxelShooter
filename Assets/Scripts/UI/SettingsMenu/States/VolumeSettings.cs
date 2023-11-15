using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class VolumeSettings : ISettingsMenuState
    {
        private readonly GameObject _settings;

        public VolumeSettings(GameObject settings)
        {
            _settings = settings;
        }

        public void Enter()
        {
            _settings.SetActive(true);
        }

        public void Exit()
        {
            _settings.SetActive(false);
        }

        public void Update()
        {
        }
    }
}
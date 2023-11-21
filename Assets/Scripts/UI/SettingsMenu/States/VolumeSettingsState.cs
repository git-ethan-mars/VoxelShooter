using UI.Carousel;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class VolumeSettingsState : ISettingsMenuState
    {
        private readonly GameObject _volumeSection;
        private readonly CarouselControl _masterVolume;
        private readonly CarouselControl _musicVolume;
        private readonly CarouselControl _soundVolume;

        public VolumeSettingsState(GameObject volumeSection, CarouselControl masterVolume, CarouselControl musicVolume,
            CarouselControl soundVolume)
        {
            _volumeSection = volumeSection;
            _masterVolume = masterVolume;
            _musicVolume = musicVolume;
            _soundVolume = soundVolume;
        }

        public void Enter()
        {
            _volumeSection.SetActive(true);
        }

        public void Exit()
        {
            _volumeSection.SetActive(false);
        }

        public void Update()
        {
        }
    }
}
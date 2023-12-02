using Data;
using Infrastructure.Services.Storage;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class VolumeSettingsState : ISettingsMenuState
    {
        private const int MinSliderValue = 0;
        private const int MaxSliderValue = 100;

        private readonly GameObject _volumeSection;
        private readonly SliderWithDisplayedValue _masterVolume;
        private readonly SliderWithDisplayedValue _musicVolume;
        private readonly SliderWithDisplayedValue _soundVolume;
        private readonly IStorageService _storageService;

        public VolumeSettingsState(GameObject volumeSection, SliderWithDisplayedValue masterVolume,
            SliderWithDisplayedValue musicVolume,
            SliderWithDisplayedValue soundVolume, IStorageService storageService)
        {
            _volumeSection = volumeSection;
            _masterVolume = masterVolume;
            _musicVolume = musicVolume;
            _soundVolume = soundVolume;
            _storageService = storageService;
            var currentSettings = _storageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey);
            _masterVolume.Construct(currentSettings.MasterVolume, MinSliderValue, MaxSliderValue);
            _musicVolume.Construct(currentSettings.MusicVolume, MinSliderValue, MaxSliderValue);
            _soundVolume.Construct(currentSettings.SoundVolume, MinSliderValue, MaxSliderValue);
        }

        public void Enter()
        {
            _volumeSection.SetActive(true);
        }

        public void Exit()
        {
            _storageService.Save(Constants.VolumeSettingsKey,
                new VolumeSettingsData((int) _masterVolume.SliderValue, (int) _musicVolume.SliderValue,
                    (int) _soundVolume.SliderValue));
            _volumeSection.SetActive(false);
        }
    }
}
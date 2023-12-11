using Infrastructure.Services.Storage;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class VolumeSettingsState : ISettingsMenuState
    {
        private const int MinSliderValue = 0;
        private const int MaxSliderValue = 100;
        private const float SliderToAudioListenerValue = 0.01f;
        private const int AudioListenerValueToSlider = 100;

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
            _masterVolume.Construct((int) currentSettings.MasterVolume * AudioListenerValueToSlider, MinSliderValue,
                MaxSliderValue);
            _musicVolume.Construct((int) currentSettings.MusicVolume * AudioListenerValueToSlider, MinSliderValue,
                MaxSliderValue);
            _soundVolume.Construct((int) currentSettings.SoundVolume * AudioListenerValueToSlider, MinSliderValue,
                MaxSliderValue);
        }

        public void Enter()
        {
            _volumeSection.SetActive(true);
            _masterVolume.SliderValue.ValueChanged += OnMasterValueChanged;
        }

        public void Exit()
        {
            _storageService.Save(Constants.VolumeSettingsKey,
                new VolumeSettingsData(_masterVolume.SliderValue.Value * SliderToAudioListenerValue,
                    _musicVolume.SliderValue.Value * SliderToAudioListenerValue,
                    _soundVolume.SliderValue.Value * SliderToAudioListenerValue));
            _masterVolume.SliderValue.ValueChanged -= OnMasterValueChanged;
            _volumeSection.SetActive(false);
        }

        private void OnMasterValueChanged(float volume)
        {
            AudioListener.volume = volume * SliderToAudioListenerValue;
        }
    }
}
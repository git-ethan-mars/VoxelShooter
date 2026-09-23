using System;
using R3;
using Services;
using UnityEngine;
namespace UI.SettingsMenuStates
{
	public class VolumeSettingsState : ISettingsMenuState
	{
		private const int MinSliderValue = 0;
		private const int MaxSliderValue = 100;
		private const float SliderToAudioListenerValue = 0.01f;
		private const int AudioListenerValueToSlider = 100;
		private readonly SliderWithDisplayedValue _masterVolume;
		private readonly SliderWithDisplayedValue _musicVolume;
		private readonly SliderWithDisplayedValue _soundVolume;
		private readonly IStorageService _storageService;

		private readonly GameObject _volumeSection;
		private IDisposable _disposable;
		private IDisposable _masterVolumeDisposable;

		public VolumeSettingsState(IStorageService storageService, GameObject volumeSection,
			SliderWithDisplayedValue masterVolume,
			SliderWithDisplayedValue musicVolume, SliderWithDisplayedValue soundVolume)
		{
			_storageService = storageService;
			_volumeSection = volumeSection;
			_masterVolume = masterVolume;
			_musicVolume = musicVolume;
			_soundVolume = soundVolume;
			var currentSettings = _storageService.Load<VolumeSettingsData>(IStorageService.VolumeSettingsKey);
			_masterVolume.Construct((int)(currentSettings.MasterVolume * AudioListenerValueToSlider), MinSliderValue,
				MaxSliderValue);
			_musicVolume.Construct((int)(currentSettings.MusicVolume * AudioListenerValueToSlider), MinSliderValue,
				MaxSliderValue);
			_soundVolume.Construct((int)(currentSettings.SoundVolume * AudioListenerValueToSlider), MinSliderValue,
				MaxSliderValue);
		}

		public void Enter()
		{
			_volumeSection.SetActive(true);

			IDisposable masterVolumeSubscription = _masterVolume.Slider.Subscribe(OnMasterValueChanged);
			IDisposable saveVolumeSubscription = Observable.Merge(_masterVolume.Slider.Select(_ => Unit.Default),
				_musicVolume.Slider.Select(_ => Unit.Default),
				_soundVolume
					.Slider
					.Select
						(_ => Unit.Default)).Subscribe(_ => _storageService.Set(new VolumeSettingsData(_masterVolume.Slider.CurrentValue * SliderToAudioListenerValue,
					_musicVolume.Slider.CurrentValue * SliderToAudioListenerValue,
					_soundVolume.Slider.CurrentValue * SliderToAudioListenerValue)));
			_disposable = Disposable.Combine(masterVolumeSubscription, saveVolumeSubscription);
		}

		public void Exit()
		{
			_disposable.Dispose();
			_volumeSection.SetActive(false);
		}

		private void OnMasterValueChanged(float volume)
		{
			AudioListener.volume = volume * SliderToAudioListenerValue;
		}
	}
}
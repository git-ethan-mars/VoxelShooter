using System;
using Data;
using Services;
using UnityEngine;
namespace GamePlay.Audio
{
	public class PlayerAudio : IDisposable
	{
		private readonly AudioSource _continuousAudio;
		private readonly float _initialStepVolume;
		private readonly AudioSource _stepAudio;
		private float _initialContinuousVolume;
		private float _soundMultiplier;
		private readonly IDisposable _disposable;

		public PlayerAudio(IStorageService storageService, AudioSource stepAudio, AudioData stepAudioData,
			AudioSource continuousAudio)
		{
			_disposable = storageService.Subscribe<VolumeSettingsData>(OnVolumeSettingsChanged);
			_stepAudio = stepAudio;
			_continuousAudio = continuousAudio;
			_stepAudio.clip = stepAudioData.clip;
			_stepAudio.volume = stepAudioData.volume;
			_stepAudio.minDistance = stepAudioData.minDistance;
			_stepAudio.maxDistance = stepAudioData.maxDistance;
			_initialStepVolume = stepAudioData.volume;
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}

		public void ChangeContinuousAudio(AudioData audio, float spatialBlend)
		{
			_continuousAudio.clip = audio.clip;
			_continuousAudio.minDistance = audio.minDistance;
			_continuousAudio.maxDistance = audio.maxDistance;
			_continuousAudio.spatialBlend = spatialBlend;
			_initialContinuousVolume = audio.volume;
			_continuousAudio.volume = _initialContinuousVolume * _soundMultiplier;
			_continuousAudio.loop = true;
			_continuousAudio.Play();
		}

		public void StopContinuousSound()
		{
			_continuousAudio.loop = false;
		}

		public void EnableStepSound()
		{
			if (!_stepAudio.isPlaying)
			{
				_stepAudio.Play();
			}

			_stepAudio.loop = true;
		}

		public void DisableStepSound()
		{
			_stepAudio.loop = false;
		}

		private void ChangeSoundMultiplier(float soundSetting)
		{
			_soundMultiplier = soundSetting;
			_stepAudio.volume = _initialStepVolume * _soundMultiplier;
			_continuousAudio.volume = _initialContinuousVolume * _soundMultiplier;
		}

		private void OnVolumeSettingsChanged(VolumeSettingsData value)
		{
			ChangeSoundMultiplier(value.SoundVolume);
		}
	}
}
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace GamePlay
{
	public class CharacterAudio : MonoBehaviour
	{
		[SerializeField] private AudioSource stepAudioSource;
		[SerializeField] private AudioSource footStepInWaterAudioSource;
		[SerializeField] private AudioSource hurtAudioSource;

		private IStorageService _storageService;

		[Inject]
		private void Construct(IStorageService storageService)
		{
			_storageService = storageService;
		}

		private void Start()
		{
			_storageService.Subscribe<VolumeSettingsData>(OnVolumeSettingsChanged)
				.AddTo(this);
		}

		public void EnableStepSound()
		{
			if (!stepAudioSource.isPlaying)
			{
				stepAudioSource.Play();
			}
		}

		public void DisableStepSound()
		{
			stepAudioSource.Stop();
		}

		public void EnableFootStepInWaterSound()
		{
			if (!footStepInWaterAudioSource.isPlaying)
			{
				footStepInWaterAudioSource.Play();
			}
			
			footStepInWaterAudioSource.loop = true;
		}

		public void DisableFootStepInWaterSound()
		{
			footStepInWaterAudioSource.loop = false;
		}

		public void PlayHurtSound()
		{
			hurtAudioSource.Play();
		}

		private void OnVolumeSettingsChanged(VolumeSettingsData volumeSettings)
		{
			stepAudioSource.volume *= volumeSettings.SoundVolume;
			hurtAudioSource.volume *= volumeSettings.SoundVolume;
			footStepInWaterAudioSource.volume *= volumeSettings.SoundVolume;
		}
	}
}
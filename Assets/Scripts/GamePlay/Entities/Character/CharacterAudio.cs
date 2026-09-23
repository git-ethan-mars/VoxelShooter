using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace GamePlay
{
	public class CharacterAudio : NetworkBehaviour
	{
		private const float MoveSpeedThreshold = 1e-3f;
		
		[SerializeField] private Character character;
		[SerializeField] private CharacterMovement movement;
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
			character.HealthSystem.Health.Pairwise()
				.Where(pair => pair.Previous > pair.Current)
				.Subscribe(_ => PlayHurtSound())
				.AddTo(this);
		}

		public override void OnStartLocalPlayer()
		{
			_storageService.Subscribe<VolumeSettingsData>(OnVolumeSettingsChanged)
				.AddTo(this);
		}
		
		private void Update()
		{
			if (movement.GetHorizontalVelocity().magnitude > MoveSpeedThreshold)
			{
				if (movement.State == MovementState.OnGround)
				{
					EnableStepSound();
					DisableFootStepInWaterSound();
				}
				if (movement.State == MovementState.OnWater)
				{
					EnableFootStepInWaterSound();
					DisableStepSound();
				}
			}
			else
			{
				DisableFootStepInWaterSound();
				DisableStepSound();
			}
		}

		private void EnableStepSound()
		{
			if (!stepAudioSource.isPlaying)
			{
				stepAudioSource.Play();
			}
		}

		private void DisableStepSound()
		{
			stepAudioSource.Stop();
		}

		private void EnableFootStepInWaterSound()
		{
			if (!footStepInWaterAudioSource.isPlaying)
			{
				footStepInWaterAudioSource.Play();
			}
			
			footStepInWaterAudioSource.loop = true;
		}

		private void DisableFootStepInWaterSound()
		{
			footStepInWaterAudioSource.loop = false;
		}

		private void PlayHurtSound()
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
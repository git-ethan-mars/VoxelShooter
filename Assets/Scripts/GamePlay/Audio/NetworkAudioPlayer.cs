using System;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Services;
using UnityEngine;
using AudioType = Data.AudioType;
namespace GamePlay.Audio
{
	public class AudioPlayer
	{
		private readonly IStaticDataService _staticData;
		private readonly AudioPool _audioPool;

		public AudioPlayer(IAssetProvider assetProvider, IStaticDataService staticData)
		{
			_staticData = staticData;
			_audioPool = new AudioPool(assetProvider);
		}

		public async UniTaskVoid Play(AudioType audioType, Vector3 position)
		{
			AudioData audioData = _staticData.GetAudioData(audioType);
			AudioSource audioSource = _audioPool.Get();
			SetupAudioSource(audioSource, audioData, true);
			audioSource.transform.position = position;
			audioSource.Play();
			
			await UniTask.Delay(TimeSpan.FromSeconds(audioData.Clip.length));

			if (audioSource != null)
			{
				_audioPool.Release(audioSource);
			}
		}
		
		public async UniTaskVoid Play(AudioType audioType, NetworkIdentity identity, bool isSpatial)
		{
			AudioData audioData = _staticData.GetAudioData(audioType);
			AudioSource audioSource = _audioPool.Get();
			SetupAudioSource(audioSource, audioData, isSpatial);
			audioSource.Play();

			Transform target = identity?.transform;
			float endTime = Time.time + audioData.Clip.length;

			while (endTime > Time.time)
			{
				if (target == null)
				{
					_audioPool.Release(audioSource);
					return;
				}
				if (audioSource == null)
				{
					return;
				}

				audioSource.transform.position = target.position;

				await UniTask.Yield();
			}

			_audioPool.Release(audioSource);
		}

		private void SetupAudioSource(AudioSource audioSource, AudioData audioData, bool isSpatial)
		{
			audioSource.resource = audioData.Clip;
			audioSource.volume = audioData.Volume;
			audioSource.minDistance = audioData.MinDistance;
			audioSource.maxDistance = audioData.MaxDistance;
			audioSource.spatialBlend = isSpatial ? 1.0f : 0.0f;
		}
	}
}
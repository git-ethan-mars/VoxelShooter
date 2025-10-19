using System;
using Data;
using Services;
using UnityEngine;
namespace GamePlay.Audio
{
	public class AudioPlayer
	{
		private const float Sound3D = 1.0f;

		private static AudioPool _audioPool;

		public AudioPlayer(IAssetProvider assets)
		{
			_audioPool = new AudioPool(assets);
		}

		public void Play(AudioData audioData, Vector3 position)
		{
			AudioSource audioSource = _audioPool.Get();
			audioSource.transform.position = position;
			audioSource.clip = audioData.clip;
			audioSource.minDistance = audioData.minDistance;
			audioSource.maxDistance = audioData.maxDistance;
			audioSource.spatialBlend = Sound3D;
			audioSource.Play();
			_audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.clip.length)).Forget();
		}

		public void Play(AudioData audioData, Transform parent, bool isSpatial)
		{
			AudioSource audioSource = _audioPool.Get();
			audioSource.transform.SetParent(parent);
			audioSource.clip = audioData.clip;
			audioSource.volume = audioData.volume;
			audioSource.minDistance = audioData.minDistance;
			audioSource.maxDistance = audioData.maxDistance;
			audioSource.spatialBlend = isSpatial ? 1.0f : 0.0f;
			audioSource.Play();
			_audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.clip.length)).Forget();
		}
	}
}
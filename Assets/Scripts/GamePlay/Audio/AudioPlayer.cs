using System;
using Common.AssetManagement;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Audio
{
	public static class AudioPlayer
	{
		private const float Sound3D = 1.0f;

		private static AudioPool _audioPool;

		public static void Initialize(IAssetProvider assets)
		{
			_audioPool = new AudioPool(assets);
		}

		public static void Play(Vector3 position, AudioData audioData)
		{
			var audioSource = _audioPool.Get();
			audioSource.transform.position = position;
			audioSource.clip = audioData.clip;
			audioSource.minDistance = audioData.minDistance;
			audioSource.maxDistance = audioData.maxDistance;
			audioSource.spatialBlend = Sound3D;
			audioSource.Play();
			_audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.clip.length)).Forget();
		}

		public static void Play(AudioData audioData, Transform parent, float spatialBlend)
		{
			var audioSource = _audioPool.Get();
			audioSource.transform.SetParent(parent);
			audioSource.clip = audioData.clip;
			audioSource.volume = audioData.volume;
			audioSource.minDistance = audioData.minDistance;
			audioSource.maxDistance = audioData.maxDistance;
			audioSource.spatialBlend = spatialBlend;
			audioSource.Play();
			_audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.clip.length)).Forget();
		}
	}
}
using System;
using Data;
using R3;
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
			audioSource.clip = audioData.Clip;
			audioSource.minDistance = audioData.MinDistance;
			audioSource.maxDistance = audioData.MaxDistance;
			audioSource.spatialBlend = Sound3D;
			audioSource.Play();
			_audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.Clip.length)).Forget();
		}

		public void Play(AudioData audioData, Transform target, bool isSpatial)
		{
			AudioSource audioSource = _audioPool.Get();
			audioSource.clip = audioData.Clip;
			audioSource.volume = audioData.Volume;
			audioSource.minDistance = audioData.MinDistance;
			audioSource.maxDistance = audioData.MaxDistance;
			audioSource.spatialBlend = isSpatial ? 1.0f : 0.0f;
			audioSource.Play();
			_audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.Clip.length)).Forget();
			Observable.EveryUpdate()
				.Subscribe(_ => audioSource.transform.position = target.position)
				.AddTo(target);
		}
	}
}
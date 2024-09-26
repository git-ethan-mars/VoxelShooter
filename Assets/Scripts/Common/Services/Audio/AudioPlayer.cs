using System;
using Common.AssetManagement;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Audio
{
	public class AudioPlayer : IAudioPlayer
	{
		private const float Sound3D = 1.0f;

		private readonly AudioPool _audioPool;

		public AudioPlayer(IAssetProvider assets)
		{
			_audioPool = new AudioPool(assets);
		}

		public async UniTask PlayAsync(Vector3 position, AudioData audioData)
		{
			var audioSource = _audioPool.Get();
			audioSource.transform.position = position;
			audioSource.clip = audioData.clip;
			audioSource.minDistance = audioData.minDistance;
			audioSource.maxDistance = audioData.maxDistance;
			audioSource.spatialBlend = Sound3D;
			audioSource.Play();
			await _audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.clip.length));
		}

		public async UniTask PlayAsync(AudioData audioData, Transform parent, float spatialBlend)
		{
			var audioSource = _audioPool.Get();
			audioSource.transform.SetParent(parent);
			audioSource.clip = audioData.clip;
			audioSource.volume = audioData.volume;
			audioSource.minDistance = audioData.minDistance;
			audioSource.maxDistance = audioData.maxDistance;
			audioSource.spatialBlend = spatialBlend;
			audioSource.Play();
			await _audioPool.ReleaseOnDelayAsync(audioSource, TimeSpan.FromSeconds(audioData.clip.length));
		}
	}
}
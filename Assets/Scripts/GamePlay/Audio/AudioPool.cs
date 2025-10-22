using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;
namespace GamePlay.Audio
{
	public class AudioPool
	{
		private const string AudioSourcePath = "Prefabs/AudioSource";
		private const string ContainerName = "AudioContainer";

		private const int PoolSize = 50;

		private readonly Stack<AudioSource> _stack;

		public AudioPool(IAssetProvider assets)
		{
			_stack = new Stack<AudioSource>(PoolSize);
			Transform container = new GameObject(ContainerName).transform;
			for (var i = 0; i < PoolSize; i++)
			{
				var audioSource = assets.Instantiate(AudioSourcePath, container).GetComponent<AudioSource>();
				audioSource.gameObject.SetActive(false);
				_stack.Push(audioSource);
			}
		}

		public AudioSource Get()
		{
			AudioSource audioSource = _stack.Pop();
			audioSource.gameObject.SetActive(true);
			return audioSource;
		}

		public async UniTaskVoid ReleaseOnDelayAsync(AudioSource audioSource, TimeSpan timeSpan)
		{
			await UniTask.Delay(timeSpan);
			Release(audioSource);
		}

		private void Release(AudioSource audioSource)
		{
			audioSource.gameObject.SetActive(false);
			audioSource.transform.position = Vector3.zero;
			_stack.Push(audioSource);
		}
	}
}
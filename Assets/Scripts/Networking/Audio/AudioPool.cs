using System.Collections.Generic;
using Services;
using UnityEngine;
namespace Networking.Audio
{
	public class AudioPool
	{
		private readonly IAssetProvider _assets;
		private const string AudioSourcePath = "Prefabs/AudioSource";
		private const string ContainerName = "AudioContainer";

		private const int PoolSize = 100;

		private readonly Stack<AudioSource> _stack = new Stack<AudioSource>(PoolSize);
		private readonly Transform _container = new GameObject(ContainerName).transform;

		public AudioPool(IAssetProvider assets)
		{
			_assets = assets;

			for (var i = 0; i < PoolSize; i++)
			{
				AddNewAudioSource();
			}
		}

		public AudioSource Get()
		{
			if (_stack.Count == 0)
			{
				AddNewAudioSource();
			}

			var audioSource = _stack.Pop();
			audioSource.gameObject.SetActive(true);

			return audioSource;
		}

		public void Release(AudioSource audioSource)
		{
			audioSource.gameObject.SetActive(false);
			audioSource.transform.position = Vector3.zero;
			_stack.Push(audioSource);
		}

		private void AddNewAudioSource()
		{
			var audioSource = _assets.Instantiate(AudioSourcePath, _container).GetComponent<AudioSource>();
			audioSource.gameObject.SetActive(false);
			_stack.Push(audioSource);
		}
	}
}
using System;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking.Messages;
using Services;
using UnityEngine;
using AudioType = Data.AudioType;
namespace Networking.Audio
{
	public class NetworkAudioPlayer
	{
		private readonly IStaticDataService _staticData;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly AudioPool _audioPool;

		public NetworkAudioPlayer(IAssetProvider assetProvider, IStaticDataService staticData, VoxelShooterNetworkManager networkManager)
		{
			_staticData = staticData;
			_networkManager = networkManager;
			_audioPool = new AudioPool(assetProvider);
		}

		public void SendAudio(AudioType audioType, Vector3 position)
		{
			var audioResponse = new StaticAudioResponse(audioType, position);
			_networkManager.SendResponseToAll(audioResponse, true);
		}

		public void SendAudio(AudioType audioType, NetworkIdentity target, bool isSpatial)
		{
			var audioResponse = new DynamicAudioResponse(audioType, target, isSpatial);
			_networkManager.SendResponseToAll(audioResponse, true);
		}

		public async void Play(AudioType audioType, Vector3 position)
		{
			AudioData audioData = _staticData.GetAudioData(audioType);
			AudioSource audioSource = _audioPool.Get();
			audioSource.transform.position = position;
			audioSource.clip = audioData.Clip;
			audioSource.minDistance = audioData.MinDistance;
			audioSource.maxDistance = audioData.MaxDistance;
			audioSource.spatialBlend = 1.0f;
			audioSource.Play();
			await UniTask.Delay(TimeSpan.FromSeconds(audioData.Clip.length));

			if (audioSource != null)
			{
				_audioPool.Release(audioSource);
			}
		}
		
		public async void Play(AudioType audioType, NetworkIdentity identity, bool isSpatial)
		{
			AudioData audioData = _staticData.GetAudioData(audioType);
			AudioSource audioSource = _audioPool.Get();
			audioSource.clip = audioData.Clip;
			audioSource.volume = audioData.Volume;
			audioSource.minDistance = audioData.MinDistance;
			audioSource.maxDistance = audioData.MaxDistance;
			audioSource.spatialBlend = isSpatial ? 1.0f : 0.0f;
			audioSource.Play();

			Transform target = identity.transform;
			float endTime = Time.time + audioData.Clip.length;

			while (endTime > Time.time)
			{
				if (audioSource == null || target == null)
				{
					return;
				}

				audioSource.transform.position = target.position;

				await UniTask.Yield();
			}

			_audioPool.Release(audioSource);
		}
	}

}
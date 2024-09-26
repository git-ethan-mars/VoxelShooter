using Common.Audio;
using Mirror;
using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<StartContinuousSoundResponse>,
		IResponseHandler<StopContinuousSoundResponse>, IResponseHandler<SurroundingSoundResponse>, IResponseHandler<PlayerSoundResponse>
	{
		private const float Sound3D = 1.0f;
		private const float Sound2D = 0.0f;

		private readonly AudioPool _audioPool;
		private float _soundMultiplier;

		public void OnResponseReceived(StartContinuousSoundResponse response)
		{
			if (response.Source != null)
			{
				var audio = _staticData.GetAudio(response.SoundId);
				var spatialBlend = Sound2D;
				if (response.Source != NetworkClient.localPlayer)
				{
					spatialBlend = Sound3D;
				}

				//response.Source.GetComponent<Player>().Audio.ChangeContinuousAudio(audio, spatialBlend);
			}
		}

		public void OnResponseReceived(StopContinuousSoundResponse response)
		{
			if (response.Source != null)
			{
			//	response.Source.GetComponent<Player>().Audio.StopContinuousSound();
			}
		}

		public void OnResponseReceived(SurroundingSoundResponse response)
		{
			var audioSource = _audioPool.Get();
			var audio = _staticData.GetAudio(response.SoundId);
			audioSource.transform.position = response.Position;
			audioSource.clip = audio.clip;
			audioSource.volume = audio.volume * _soundMultiplier;
			audioSource.minDistance = audio.minDistance;
			audioSource.maxDistance = audio.maxDistance;
			audioSource.spatialBlend = Sound3D;
			audioSource.Play();
			_coroutineRunner.StartCoroutine(_audioPool.ReleaseOnDelay(audioSource, audio.clip.length));
		}

		public void OnResponseReceived(PlayerSoundResponse response)
		{
			if (response.Source != null)
			{
				var audioSource = _audioPool.Get();
				var transformFollower = audioSource.GetComponent<TransformFollower>();
				transformFollower.Target = response.Source.transform;
				transformFollower.enabled = true;
				var audio = _staticData.GetAudio(response.SoundId);
				audioSource.clip = audio.clip;
				audioSource.volume = audio.volume * _soundMultiplier;
				audioSource.minDistance = audio.minDistance;
				audioSource.maxDistance = audio.maxDistance;
				audioSource.spatialBlend = response.Source == NetworkClient.localPlayer ? Sound2D : Sound3D;
				audioSource.Play();
				_coroutineRunner.StartCoroutine(_audioPool.ReleaseOnDelay(audioSource, audio.clip.length));
			}
		}
	}
}
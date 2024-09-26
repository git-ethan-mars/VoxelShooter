using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<RchParticleResponse>, IResponseHandler<StartMuzzleFlashResponse>, IResponseHandler<StopMuzzleFlashResponse>
	{
		public void OnResponseReceived(RchParticleResponse response)
		{
			_particleFactory.CreateRchParticle(response.Position, response.ParticleSpeed, response.ParticlesCount);
		}

		public void OnResponseReceived(StartMuzzleFlashResponse response)
		{
			if (response.Source != null)
			{
				var particleSystem = response.Source.gameObject.GetComponentInChildren<ParticleSystem>();
				particleSystem.Play();
			}
		}

		public void OnResponseReceived(StopMuzzleFlashResponse response)
		{
			if (response.Source != null)
			{
				var particleSystem = response.Source.gameObject.GetComponentInChildren<ParticleSystem>();
				particleSystem.Stop();
			}
		}
	}
}
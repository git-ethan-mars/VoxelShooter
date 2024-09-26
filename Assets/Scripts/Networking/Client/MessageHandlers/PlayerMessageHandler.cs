using Networking.Messages.Responses;

namespace Networking.Client
{
	public partial class MirrorClient : IResponseHandler<HealthResponse>
	{
		public void OnResponseReceived(HealthResponse response)
		{
			/*if (NetworkClient.connection.identity != null &&
			    NetworkClient.connection.identity.TryGetComponent<Player>(out var player))
			{
				player.Health.Value = response.Health;
			}*/
		}
	}
}
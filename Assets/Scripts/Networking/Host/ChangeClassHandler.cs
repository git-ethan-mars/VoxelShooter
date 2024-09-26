using Mirror;
using Networking.Messages.Requests;

namespace Networking.Host
{
	public  partial class MirrorHost : IRequestHandler<ChangeClassRequest>
	{
		public void OnRequestReceived(NetworkConnectionToClient connection, ChangeClassRequest request)
		{
			var result = TryGetPlayerData(connection, out var playerData);
			if (!result)
			{
				return;
			}

			if (playerData.GameClass != request.GameClass)
			{
				ChangeClass(connection, request.GameClass);
			}
		}
	}
}
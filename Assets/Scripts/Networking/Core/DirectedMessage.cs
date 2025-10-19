using Mirror;
namespace Networking.Core
{
	public class DirectedMessage
	{
		public readonly NetworkConnectionToClient Connection;
		public readonly NetworkMessage Message;
		
		public DirectedMessage(NetworkConnectionToClient connection, NetworkMessage message)
		{
			Connection = connection;
			Message = message;
		}

		public DirectedMessage(NetworkMessage message)
		{
			Message = message;
		}
	}
	
}
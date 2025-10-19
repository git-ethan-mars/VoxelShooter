using Mirror;
namespace Networking.Core
{
	public class DirectedMessage<TMessage> : DirectedMessage where TMessage : struct, NetworkMessage
	{
		public new TMessage Message { get; }

		public DirectedMessage(NetworkConnectionToClient connection, TMessage message) : base(connection, message)
		{
			Message = message;
		}

		public DirectedMessage(TMessage message) : base(message)
		{
			Message = message;
		}
	}
}
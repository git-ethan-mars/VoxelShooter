using Mirror;
namespace Networking.Messages
{
	public struct StopContinuousSoundResponse : NetworkMessage
	{
		public readonly NetworkIdentity Source;

		public StopContinuousSoundResponse(NetworkIdentity source)
		{
			Source = source;
		}
	}
}
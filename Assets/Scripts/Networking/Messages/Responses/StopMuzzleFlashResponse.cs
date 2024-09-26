using Mirror;

namespace Networking.Messages.Responses
{
	public struct StopMuzzleFlashResponse : IMirrorResponse
	{
		public readonly NetworkIdentity Source;

		public StopMuzzleFlashResponse(NetworkIdentity source)
		{
			Source = source;
		}
	}
}
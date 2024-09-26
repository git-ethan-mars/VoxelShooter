using Mirror;

namespace Networking.Messages.Responses
{
	public struct StartMuzzleFlashResponse : IMirrorResponse
	{
		public readonly NetworkIdentity Source;

		public StartMuzzleFlashResponse(NetworkIdentity source)
		{
			Source = source;
		}
	}
}
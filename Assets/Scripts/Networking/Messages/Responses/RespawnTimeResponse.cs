using GamePlay.Data;

namespace Networking.Messages.Responses
{
	public struct RespawnTimeResponse : IMirrorResponse
	{
		public ServerTime TimeLeft;

		public RespawnTimeResponse(ServerTime timeLeft)
		{
			TimeLeft = timeLeft;
		}
	}
}
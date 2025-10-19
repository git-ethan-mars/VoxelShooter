using System;
using Mirror;
namespace Networking.Messages
{
	public struct RespawnTimeResponse : NetworkMessage
	{
		public TimeSpan TimeLeft;

		public RespawnTimeResponse(TimeSpan timeLeft)
		{
			TimeLeft = timeLeft;
		}
	}
}
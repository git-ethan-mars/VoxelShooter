using System;
using Mirror;
namespace Networking.Messages
{
	public struct GameTimeResponse : NetworkMessage
	{
		public readonly TimeSpan TimeLeft;

		public GameTimeResponse(TimeSpan timeLeft)
		{
			TimeLeft = timeLeft;
		}
	}
}
using System;
namespace Networking.Messages
{
	public struct GameTimeResponse : IResponse
	{
		public readonly TimeSpan TimeLeft;

		public GameTimeResponse(TimeSpan timeLeft)
		{
			TimeLeft = timeLeft;
		}
	}
}
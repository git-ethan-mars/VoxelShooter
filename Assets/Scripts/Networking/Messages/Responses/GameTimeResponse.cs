using Common.StaticData;

namespace Networking.Messages.Responses
{
	public struct GameTimeResponse : IMirrorResponse
	{
		public readonly ServerTime TimeLeft;

		public GameTimeResponse(ServerTime timeLeft)
		{
			TimeLeft = timeLeft;
		}
	}
}
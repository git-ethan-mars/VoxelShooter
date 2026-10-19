using Data;

namespace Networking.Messages
{
	public struct ScoreboardResponse : IResponse
	{
		public readonly ScoreboardEntry[] Entries;

		public ScoreboardResponse(ScoreboardEntry[] entries)
		{
			Entries = entries;
		}
	}
}

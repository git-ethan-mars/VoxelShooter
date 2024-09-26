using System.Collections.Generic;
using Common.StaticData;

namespace Networking.Messages.Responses
{
	public struct ScoreboardResponse : IMirrorResponse
	{
		public readonly List<ScoreData> Scores;

		public ScoreboardResponse(List<ScoreData> scores)
		{
			Scores = scores;
		}
	}
}
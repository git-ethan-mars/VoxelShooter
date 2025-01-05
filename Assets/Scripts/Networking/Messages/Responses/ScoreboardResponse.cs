using System.Collections.Generic;
using GamePlay.Data;

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
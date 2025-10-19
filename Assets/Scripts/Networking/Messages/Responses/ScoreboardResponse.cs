using System.Collections.Generic;
using Data;
using Mirror;
namespace Networking.Messages
{
	public struct ScoreboardResponse : NetworkMessage
	{
		public readonly List<PlayerData> Scores;

		public ScoreboardResponse(List<PlayerData> scores)
		{
			Scores = scores;
		}
	}
}
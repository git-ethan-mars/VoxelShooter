using System.Collections.Generic;
using Data;
using Mirror;
namespace Networking.Messages
{
	public struct ScoreboardResponse : NetworkMessage
	{
		public readonly List<DeathMatchPlayerData> Scores;

		public ScoreboardResponse(List<DeathMatchPlayerData> scores)
		{
			Scores = scores;
		}
	}
}
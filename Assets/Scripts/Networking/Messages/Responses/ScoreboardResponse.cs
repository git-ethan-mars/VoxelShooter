using System.Collections.Generic;
using Data;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ScoreboardResponse : NetworkMessage
    {
        public readonly List<ScoreData> Scores;
        public ScoreboardResponse(List<ScoreData> scores)
        {
            Scores = scores;
        }
    }
}
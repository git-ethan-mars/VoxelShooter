using System.Collections.Generic;
using Data;
using Mirror;

namespace Networking.Messages
{
    public struct ScoreboardMessage : NetworkMessage
    {
        public readonly List<ScoreData> Scores;
        public ScoreboardMessage(List<ScoreData> scores)
        {
            Scores = scores;
        }
    }
}
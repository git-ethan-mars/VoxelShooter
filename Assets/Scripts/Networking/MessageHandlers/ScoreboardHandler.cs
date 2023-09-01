using System;
using System.Collections.Generic;
using Data;
using Networking.Messages.Responses;

namespace Networking
{
    public class ScoreboardHandler : MessageHandler<ScoreboardResponse>
    {
        public event Action<List<ScoreData>> ScoreboardChanged;

        protected override void OnMessageReceived(ScoreboardResponse message)
        {
            ScoreboardChanged?.Invoke(message.Scores);
        }
    }
}
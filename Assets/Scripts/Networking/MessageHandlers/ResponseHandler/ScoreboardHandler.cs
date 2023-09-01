using System;
using System.Collections.Generic;
using Data;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ScoreboardHandler : ResponseHandler<ScoreboardResponse>
    {
        public event Action<List<ScoreData>> ScoreboardChanged;

        protected override void OnResponseReceived(ScoreboardResponse message)
        {
            ScoreboardChanged?.Invoke(message.Scores);
        }
    }
}
using System;
using Data;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class GameTimeHandler : ResponseHandler<GameTimeResponse>
    {
        public event Action<ServerTime> GameTimeChanged;

        protected override void OnResponseReceived(GameTimeResponse message)
        {
            GameTimeChanged?.Invoke(message.TimeLeft);
        }
    }
}
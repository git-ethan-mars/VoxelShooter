using System;
using Data;
using Networking.Messages.Responses;

namespace Networking
{
    public class GameTimeHandler : MessageHandler<GameTimeResponse>
    {
        public event Action<ServerTime> GameTimeChanged;

        protected override void OnMessageReceived(GameTimeResponse message)
        {
            GameTimeChanged?.Invoke(message.TimeLeft);
        }
    }
}
using System;
using Data;
using Networking.Messages.Responses;

namespace Networking
{
    public class RespawnTimeHandler : MessageHandler<RespawnTimeResponse>
    {
        public event Action<ServerTime> RespawnTimeChanged;

        protected override void OnMessageReceived(RespawnTimeResponse message)
        {
            RespawnTimeChanged?.Invoke(message.TimeLeft);
        }
    }
}
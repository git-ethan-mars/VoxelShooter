using System;
using Data;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class RespawnTimeHandler : ResponseHandler<RespawnTimeResponse>
    {
        public event Action<ServerTime> RespawnTimeChanged;

        protected override void OnResponseReceived(RespawnTimeResponse message)
        {
            RespawnTimeChanged?.Invoke(message.TimeLeft);
        }
    }
}
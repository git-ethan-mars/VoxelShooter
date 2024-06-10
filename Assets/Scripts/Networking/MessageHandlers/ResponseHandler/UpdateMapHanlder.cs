using System;
using System.Collections.Generic;
using Data;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class UpdateMapHandler : ResponseHandler<UpdateMapResponse>
    {
        public event Action<BlockDataWithPosition[]> MapUpdated;
        private readonly IClient _client;
        private readonly List<UpdateMapResponse> _mapUpdateBuffer;

        public UpdateMapHandler(IClient client)
        {
            _client = client;
            _mapUpdateBuffer = new List<UpdateMapResponse>();
        }

        protected override void OnResponseReceived(UpdateMapResponse message)
        {
            _mapUpdateBuffer.Add(message);
            if (!_client.MapLoadingProgress.IsMapLoaded)
            {
                return;
            }

            _mapUpdateBuffer.ForEach(msg => MapUpdated?.Invoke(msg.Blocks));
            _mapUpdateBuffer.Clear();
        }
    }
}
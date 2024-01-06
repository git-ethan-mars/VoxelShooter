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
        private bool _canHandle;

        public UpdateMapHandler(IClient client)
        {
            _client = client;
            _client.MapDownloaded += OnMapDownloaded;
            _mapUpdateBuffer = new List<UpdateMapResponse>();
            _canHandle = false;
        }

        protected override void OnResponseReceived(UpdateMapResponse message)
        {
            _mapUpdateBuffer.Add(message);
            if (!_canHandle)
            {
                return;
            }

            _mapUpdateBuffer.ForEach(msg => MapUpdated?.Invoke(msg.Blocks));
            _mapUpdateBuffer.Clear();
        }

        private void OnMapDownloaded()
        {
            _client.MapDownloaded -= OnMapDownloaded;
            _canHandle = true;
        }
    }
}
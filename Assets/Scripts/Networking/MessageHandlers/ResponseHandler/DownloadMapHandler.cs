using System;
using System.Collections.Generic;
using System.IO;
using Generators;
using MapLogic;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class DownloadMapHandler : ResponseHandler<DownloadMapResponse>
    {
        public event Action<float> MapLoadProgressed;
        public event Action MapDownloaded;
        private readonly List<byte> _byteChunks = new();
        private readonly Client _client;

        public DownloadMapHandler(Client client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(DownloadMapResponse message)
        {
            _byteChunks.AddRange(message.ByteChunk);
            MapLoadProgressed?.Invoke(message.Progress);
            if (message.Progress != 1) return;
            _client.MapProvider = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()));
            _client.MapGenerator = new MapGenerator(_client);
            MapDownloaded?.Invoke();
        }
    }
}
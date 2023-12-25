using System;
using System.Collections.Generic;
using System.IO;
using Infrastructure.Services.StaticData;
using MapLogic;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class DownloadMapHandler : ResponseHandler<DownloadMapResponse>
    {
        public event Action<float> MapLoadProgressed;
        private readonly List<byte> _byteChunks = new();
        private readonly IClient _client;
        private readonly IStaticDataService _staticData;

        public DownloadMapHandler(IClient client, IStaticDataService staticData)
        {
            _client = client;
            _staticData = staticData;
        }

        protected override void OnResponseReceived(DownloadMapResponse message)
        {
            _byteChunks.AddRange(message.ByteChunk);
            MapLoadProgressed?.Invoke(message.Progress);
            if (message.Progress != 1)
            {
                return;
            }

            var mapConfigure = _staticData.GetMapConfigure(_client.MapName);
            var mapData = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()), mapConfigure);
            _client.MapProvider = new MapProvider(mapData, mapConfigure);
        }
    }
}
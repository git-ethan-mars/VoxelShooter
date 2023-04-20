using System;
using System.Collections.Generic;
using System.IO;
using Data;
using MapLogic;
using Networking.Messages;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MapMessageHandler
    {
        public event Action<Map> MapDownloaded;
        public Dictionary<Vector3Int, BlockData> Buffer { get; set; } = new();
        public Map Map { get; set; }
        private readonly List<byte> _byteChunks = new();
        private MapRenderer _mapRenderer;



        public void OnMapDownloadMessage(DownloadMapMessage mapMessage)
        {
            _byteChunks.AddRange(mapMessage.ByteChunk);
            if (!mapMessage.IsFinalChunk) return;
            Map = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()));
            MapDownloaded?.Invoke(Map);
        }

        public void OnMapUpdateMessage(UpdateMapMessage message)
        {
            if (message.BlockData.Length != message.Positions.Length)
            {
                throw new ArgumentException("Message was truncated");
            }

            for (var i = 0; i < message.Positions.Length; i++)
            {
                Buffer[message.Positions[i]] = message.BlockData[i];
            }
            
        }
    }
}
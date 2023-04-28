using System;
using System.Collections.Generic;
using System.IO;
using Data;
using MapLogic;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking
{
    public class ClientMessagesHandler
    {
        public event Action<Map, Dictionary<Vector3Int, BlockData>> MapDownloaded;  
        public readonly Dictionary<Vector3Int, BlockData> MapUpdates;
        private readonly List<byte> _byteChunks = new();
        
        public ClientMessagesHandler()
        {
            MapUpdates = new Dictionary<Vector3Int, BlockData>();
        }
        public void RegisterHandlers()
        {
            NetworkClient.RegisterHandler<DownloadMapMessage>(OnMapDownloadMessage);
            NetworkClient.RegisterHandler<UpdateMapMessage>(OnMapUpdateMessage);
        }

        public void RemoveHandlers()
        {
            NetworkClient.UnregisterHandler<DownloadMapMessage>();
            NetworkClient.UnregisterHandler<UpdateMapMessage>();
        }

        private void OnMapDownloadMessage(DownloadMapMessage mapMessage)
        {
            _byteChunks.AddRange(mapMessage.ByteChunk);
            if (!mapMessage.IsFinalChunk) return;
            var map = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()));
            MapDownloaded?.Invoke(map, MapUpdates);
        }

        private void OnMapUpdateMessage(UpdateMapMessage message)
        {
            if (message.BlockData.Length != message.Positions.Length)
            {
                throw new ArgumentException("Message was truncated");
            }

            for (var i = 0; i < message.Positions.Length; i++)
            {
                MapUpdates[message.Positions[i]] = message.BlockData[i];
            }
            
        }
    }
}
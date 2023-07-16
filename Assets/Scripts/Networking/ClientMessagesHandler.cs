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
        public event Action<ServerTime> ServerTimeUpdated;
        public event Action<ServerTime> RespawnTimeUpdated; 
        public event Action<MapProvider, Dictionary<Vector3Int, BlockData>> MapDownloaded;
        public event Action<List<ScoreData>> ScoreboardUpdated;
        public event Action<float> MapProgress;
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
            NetworkClient.RegisterHandler<ServerTimeMessage>(OnServerTimeMessage);
            NetworkClient.RegisterHandler<RespawnTimeMessage>(OnRespawnTimeMessage);
            NetworkClient.RegisterHandler<ScoreboardMessage>(OnScoreboardMessage);
        }

        public void RemoveHandlers()
        {
            NetworkClient.UnregisterHandler<DownloadMapMessage>();
            NetworkClient.UnregisterHandler<UpdateMapMessage>();
            NetworkClient.UnregisterHandler<ServerTimeMessage>();
            NetworkClient.UnregisterHandler<RespawnTimeMessage>();
            NetworkClient.UnregisterHandler<ScoreboardMessage>();
        }

        private void OnMapDownloadMessage(DownloadMapMessage mapMessage)
        {
            _byteChunks.AddRange(mapMessage.ByteChunk);
            MapProgress?.Invoke(mapMessage.Progress);
            if (mapMessage.Progress != 1) return;
            var mapProvider = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()));
            MapDownloaded?.Invoke(mapProvider, MapUpdates);
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

        private void OnServerTimeMessage(ServerTimeMessage message)
        {
            ServerTimeUpdated?.Invoke(message.TimeLeft);
        }

        private void OnRespawnTimeMessage(RespawnTimeMessage message)
        {
            RespawnTimeUpdated?.Invoke(message.TimeLeft);
        }

        private void OnScoreboardMessage(ScoreboardMessage message)
        {
            ScoreboardUpdated?.Invoke(message.Scores);
        }
    }
}
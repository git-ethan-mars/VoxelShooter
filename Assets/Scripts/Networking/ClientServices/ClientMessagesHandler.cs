using System;
using System.Collections.Generic;
using System.IO;
using Data;
using MapLogic;
using Mirror;
using Networking.Messages;
using Networking.Messages.Responses;

namespace Networking
{
    public class ClientMessagesHandler
    {
        private readonly Action<float> _mapProgressCallback;
        private readonly Action _mapDownloadedCallback;
        private readonly Action<ServerTime> _serverTimeUpdatedCallback;
        private readonly Action<ServerTime> _respawnTimeUpdatedCallback;
        private readonly Action<List<ScoreData>> _scoreBoardUpdatedCallback;
        private readonly ClientData _client;
        private readonly List<byte> _byteChunks = new();

        public ClientMessagesHandler(ClientData client, Action<float> mapProgress, Action mapDownloaded,
            Action<ServerTime> serverTimeUpdated, Action<ServerTime> respawnTimeUpdated,
            Action<List<ScoreData>> scoreBoardUpdated)
        {
            _client = client;
            _mapProgressCallback = mapProgress;
            _mapDownloadedCallback = mapDownloaded;
            _serverTimeUpdatedCallback = serverTimeUpdated;
            _respawnTimeUpdatedCallback = respawnTimeUpdated;
            _scoreBoardUpdatedCallback = scoreBoardUpdated;
        }

        public void RegisterHandlers()
        {
            NetworkClient.RegisterHandler<DownloadMapResponse>(OnMapDownloadResponse);
            NetworkClient.RegisterHandler<UpdateMapResponse>(OnMapUpdateResponse);
            NetworkClient.RegisterHandler<ServerTimeResponse>(OnServerTimeResponse);
            NetworkClient.RegisterHandler<RespawnTimeResponse>(OnRespawnTimeResponse);
            NetworkClient.RegisterHandler<ScoreboardResponse>(OnScoreboardResponse);
        }

        public void RemoveHandlers()
        {
            NetworkClient.UnregisterHandler<DownloadMapResponse>();
            NetworkClient.UnregisterHandler<UpdateMapResponse>();
            NetworkClient.UnregisterHandler<FallBlockResponse>();
            NetworkClient.UnregisterHandler<ServerTimeResponse>();
            NetworkClient.UnregisterHandler<RespawnTimeResponse>();
            NetworkClient.UnregisterHandler<ScoreboardResponse>();
        }

        private void OnMapDownloadResponse(DownloadMapResponse mapResponse)
        {
            _byteChunks.AddRange(mapResponse.ByteChunk);
            _mapProgressCallback?.Invoke(mapResponse.Progress);
            if (mapResponse.Progress != 1) return;
            _client.MapProvider = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()));
            _mapDownloadedCallback?.Invoke();
        }

        private void OnMapUpdateResponse(UpdateMapResponse response)
        {
            _client.BufferToUpdateMap.Add(response);
        }

        private void OnServerTimeResponse(ServerTimeResponse response)
        {
            _serverTimeUpdatedCallback?.Invoke(response.TimeLeft);
        }

        private void OnRespawnTimeResponse(RespawnTimeResponse message)
        {
            _respawnTimeUpdatedCallback?.Invoke(message.TimeLeft);
        }

        private void OnScoreboardResponse(ScoreboardResponse message)
        {
            _scoreBoardUpdatedCallback?.Invoke(message.Scores);
        }
    }
}
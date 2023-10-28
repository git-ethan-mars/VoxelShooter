using System.Collections.Generic;
using Data;
using Networking.ClientServices;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class UpdateMapHandler : ResponseHandler<UpdateMapResponse>
    {
        private readonly IClient _client;

        public UpdateMapHandler(IClient client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(UpdateMapResponse message)
        {
            _client.Data.BufferToUpdateMap.Add(message);
            if (_client.Data.State == ClientState.Connecting)
                return;
            var dataByChunkIndex = new Dictionary<int, List<(Vector3Int, BlockData)>>();
            for (var i = 0; i < _client.Data.BufferToUpdateMap.Count; i++)
            {
                for (var j = 0; j < _client.Data.BufferToUpdateMap[i].Positions.Length; j++)
                {
                    var chunkIndex =
                        _client.MapProvider.GetChunkNumberByGlobalPosition(_client.Data.BufferToUpdateMap[i]
                            .Positions[j]);
                    if (!dataByChunkIndex.ContainsKey(chunkIndex))
                    {
                        dataByChunkIndex[chunkIndex] = new List<(Vector3Int, BlockData)>();
                    }

                    var localPosition =
                        _client.MapProvider.GetLocalPositionByGlobal(_client.Data.BufferToUpdateMap[i].Positions[j]);
                    dataByChunkIndex[chunkIndex].Add((localPosition, _client.Data.BufferToUpdateMap[i].BlockData[j]));
                }
            }

            foreach (var (chunkIndex, data) in dataByChunkIndex)
            {
                _client.MapGenerator.ChunkGenerators[chunkIndex].SpawnBlocks(data);
            }

            _client.Data.BufferToUpdateMap.Clear();
        }
    }
}
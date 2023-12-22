using System.Collections.Generic;
using Data;
using Networking.ClientServices;
using Networking.Messages.Responses;
using Networking.ServerServices;

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
            var blocksByChunkIndex = new Dictionary<int, List<BlockDataWithPosition>>();
            for (var i = 0; i < _client.Data.BufferToUpdateMap.Count; i++)
            {
                for (var j = 0; j < _client.Data.BufferToUpdateMap[i].Blocks.Length; j++)
                {
                    var chunkIndex =
                        _client.MapProvider.GetChunkNumberByGlobalPosition(_client.Data.BufferToUpdateMap[i].Blocks[j]
                            .Position);
                    if (!blocksByChunkIndex.ContainsKey(chunkIndex))
                    {
                        blocksByChunkIndex[chunkIndex] = new List<BlockDataWithPosition>();
                    }

                    var localPosition =
                        _client.MapProvider.GetLocalPositionByGlobal(_client.Data.BufferToUpdateMap[i].Blocks[j]
                            .Position);
                    blocksByChunkIndex[chunkIndex].Add(new BlockDataWithPosition(localPosition,
                        _client.Data.BufferToUpdateMap[i].Blocks[j].BlockData));
                }
            }

            foreach (var (chunkIndex, blocks) in blocksByChunkIndex)
            {
                _client.ChunkMeshProvider.UpdateChunk(chunkIndex, blocks);
            }

            _client.Data.BufferToUpdateMap.Clear();
        }
    }
}
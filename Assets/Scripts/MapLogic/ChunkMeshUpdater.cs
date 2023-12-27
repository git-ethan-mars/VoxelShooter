using System.Linq;
using Data;
using Generators;
using Networking;

namespace MapLogic
{
    public class ChunkMeshUpdater
    {
        private readonly ChunkMesh[] _chunkMeshes;
        private readonly IClient _client;

        public ChunkMeshUpdater(IClient client, ChunkMesh[] chunkMeshes)
        {
            _chunkMeshes = chunkMeshes;
            _client = client;
            client.MapUpdated += OnMapUpdated;
        }

        public void Dispose()
        {
            _client.MapUpdated -= OnMapUpdated;
        }

        private void OnMapUpdated(BlockDataWithPosition[] updatedBlocks)
        {
            var blocksByChunkIndex = updatedBlocks
                .GroupBy(data => _client.MapProvider.GetChunkNumberByGlobalPosition(data.Position),
                    data =>
                        new BlockDataWithPosition(_client.MapProvider.GetLocalPositionByGlobal(data.Position),
                            data.BlockData))
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var (chunkIndex, blocks) in blocksByChunkIndex)
            {
                _chunkMeshes[chunkIndex].SpawnBlocks(blocks);
            }
        }
    }
}
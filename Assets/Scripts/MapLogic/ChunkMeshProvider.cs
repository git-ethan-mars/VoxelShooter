using System.Collections.Generic;
using Data;
using Generators;

namespace MapLogic
{
    public class ChunkMeshProvider
    {
        private readonly ChunkMesh[] _chunkMeshes;

        public ChunkMeshProvider(ChunkMesh[] chunkMeshes)
        {
            _chunkMeshes = chunkMeshes;
        }

        public void UpdateChunk(int chunkIndex, List<BlockDataWithPosition> blocks)
        {
            _chunkMeshes[chunkIndex].SpawnBlocks(blocks);
        }
    }
}
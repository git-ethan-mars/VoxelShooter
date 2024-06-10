using System;
using System.Linq;
using Data;
using Generators;
using Networking;

namespace MapLogic
{
    public class MapMeshUpdater
    {
        private readonly ChunkMesh[] _chunkMeshes;
        private readonly IMapProvider _mapProvider;

        public MapMeshUpdater(ChunkMesh[] chunkMeshes, IMapProvider mapProvider)
        {
            _chunkMeshes = chunkMeshes;
            _mapProvider = mapProvider;
        }

        public void UpdateMesh(BlockDataWithPosition[] updatedBlocks)
        {
            var blocksByChunkIndex = updatedBlocks
                .GroupBy(data => _mapProvider.GetChunkNumberByGlobalPosition(data.Position),
                    data =>
                        new BlockDataWithPosition(_mapProvider.GetLocalPositionByGlobal(data.Position),
                            data.BlockData))
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var (chunkIndex, blocks) in blocksByChunkIndex)
            {
                _chunkMeshes[chunkIndex].SpawnBlocks(blocks);
            }
        }
    }
}
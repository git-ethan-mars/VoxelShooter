using Data;
using UnityEngine;

namespace MapLogic
{
    public interface IMapProvider
    {
        BlockData GetBlockByGlobalPosition(int x, int y, int z);
        BlockData GetBlockByGlobalPosition(Vector3Int position);
        void SetBlockByGlobalPosition(int x, int y, int z, BlockData block);
        void SetBlockByGlobalPosition(Vector3Int position, BlockData block);
        bool IsInsideMap(int x, int y, int z);
        bool IsDestructiblePosition(Vector3Int pool);
        int Depth { get; }
        int Height { get; }
        int Width { get; }
        Color32 WaterColor { get; }
        Color32 SolidColor { get; }
        int ChunkCount { get; }
        int GetChunkNumberByGlobalPosition(Vector3Int position);
        Vector3Int GetLocalPositionByGlobal(Vector3Int position);
        ChunkData GetChunkByIndex(int p0);
    }
}
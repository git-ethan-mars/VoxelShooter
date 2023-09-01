using System;
using Data;
using UnityEngine;

namespace MapLogic
{
    public interface IMapProvider : IDisposable
    {
        BlockData GetBlockByGlobalPosition(Vector3Int position);
        BlockData GetBlockByGlobalPosition(int x, int y, int z);
        int FindChunkNumberByPosition(int x, int y, int z);
        int FindChunkNumberByPosition(Vector3Int position);
        bool IsValidPosition(Vector3Int globalPosition);
        MapData MapData { get; }
    }
}
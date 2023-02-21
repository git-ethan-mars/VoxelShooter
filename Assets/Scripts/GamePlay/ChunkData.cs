using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class ChunkData
    {
        public readonly Block[,,] Blocks =
            new Block[ChunkSize, ChunkSize, ChunkSize];

        public Dictionary<Vector3Int, SpawnerType> SpawnerTypeByLocalPosition = new();
        public Dictionary<Vector3Int, DropType> DropTypeByLocalPosition = new();
        public const int ChunkSize = 32;
    }
}
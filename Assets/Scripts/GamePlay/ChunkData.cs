using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class ChunkData
    {
        public readonly Block[][][] Blocks;
           

        //public Dictionary<Vector3Int, SpawnerType> SpawnerTypeByLocalPosition = new();
        //public Dictionary<Vector3Int, DropType> DropTypeByLocalPosition = new();
        public const int ChunkSize = 32;

        public ChunkData()
        {
            Blocks = new Block[ChunkSize][][];
            for (var i = 0; i < ChunkSize; i++)
            {
                Blocks[i] = new Block[ChunkSize][];
                for (var j = 0; j < ChunkSize; j++)
                {
                    Blocks[i][j] = new Block[ChunkSize];
                }
            }
        }
    }
}
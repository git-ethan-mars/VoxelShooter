using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class MapData
    {
        public readonly Color32 SolidColor = new(89, 53, 47, 255);
        public readonly int Width;
        public readonly int Depth;
        public readonly int Height;
        public readonly ChunkData[] Chunks;
        public readonly List<SpawnPoint> SpawnPoints;

        public MapData(ChunkData[] chunks, int width, int height, int depth, List<SpawnPoint> spawnPoints)
        {
            Chunks = chunks;
            Width = width;
            Height = height;
            Depth = depth;
            SpawnPoints = spawnPoints;
        }
    }
}
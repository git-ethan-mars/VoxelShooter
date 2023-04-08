using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class MapData
    {
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

    public struct SpawnPoint
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3 ToUnityVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class MapData
    {
        public readonly Color32 SolidColor;
        public readonly Color32 WaterColor;
        public readonly int Width;
        public readonly int Depth;
        public readonly int Height;
        public readonly ChunkData[] Chunks;
        public readonly List<Vector3Int> BoxSpawnLayer = new();

        public MapData(ChunkData[] chunks, int width, int height, int depth, Color32 solidColor, Color32 waterColor)
        {
            Chunks = chunks;
            Width = width;
            Height = height;
            Depth = depth;
            SolidColor = solidColor;
            WaterColor = waterColor;
        }
    }
}
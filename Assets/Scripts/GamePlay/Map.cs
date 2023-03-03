using UnityEngine;

namespace GamePlay
{
    public class Map
    {
        public readonly int Width;
        public readonly int Depth;
        public readonly int Height;
        public readonly ChunkData[] Chunks;
        public Map(ChunkData[] chunks, int width, int height, int depth)
        {
            Chunks = chunks;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public int FindChunkByPosition(Vector3Int position)
        {
            return position.z / ChunkData.ChunkSize +
                   position.y / ChunkData.ChunkSize * (Depth / ChunkData.ChunkSize) +
                   position.x / ChunkData.ChunkSize * (Height / ChunkData.ChunkSize * Depth / ChunkData.ChunkSize);
        }
    }
}

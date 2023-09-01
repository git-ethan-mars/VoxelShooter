using System;
using Unity.Collections;

namespace Data
{
    public class ChunkData : IDisposable
    {
        public NativeArray<Faces> Faces = new(ChunkSizeCubed, Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);

        public NativeArray<BlockData> Blocks = new(ChunkSizeCubed, Allocator.Persistent);
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;

        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkSize && y is >= 0 and < ChunkSize &&
                   z is >= 0 and < ChunkSize;
        }

        public void Dispose()
        {
            Faces.Dispose();
            Blocks.Dispose();
        }
    }
}
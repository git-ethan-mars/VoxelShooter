using Unity.Collections;

namespace Data
{
    public class ChunkData
    {
        public NativeArray<Faces> Faces = new(ChunkSizeCubed, Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);

        public NativeArray<BlockData> Blocks = new(ChunkSizeCubed, Allocator.Persistent);
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;

        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkData.ChunkSize && y is >= 0 and < ChunkData.ChunkSize &&
                   z is >= 0 and < ChunkData.ChunkSize;
        }
    }
}
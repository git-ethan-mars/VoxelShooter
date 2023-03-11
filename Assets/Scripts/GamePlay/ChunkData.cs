using Unity.Collections;

namespace GamePlay
{
    public class ChunkData
    {
        public NativeArray<Block> Blocks = new(ChunkSizeCubed, Allocator.Persistent);
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;
    }
}
namespace GamePlay
{
    public class ChunkData
    {
        public readonly Block[,,] Blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;

    }
}
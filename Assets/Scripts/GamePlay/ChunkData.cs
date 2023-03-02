namespace GamePlay
{
    public class ChunkData
    {
        public readonly Block[,,] Blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
        public const int ChunkSize = 32;

    }
}
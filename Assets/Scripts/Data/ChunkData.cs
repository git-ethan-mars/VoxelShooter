namespace Data
{
    public class ChunkData
    {
        public Faces[] Faces = new Faces[ChunkSizeCubed];

        public readonly BlockData[] Blocks = new BlockData[ChunkSizeCubed];
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;

        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkSize && y is >= 0 and < ChunkSize &&
                   z is >= 0 and < ChunkSize;
        }
    }
}
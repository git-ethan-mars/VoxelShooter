namespace Data
{
    public class ChunkData
    {
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;

        public readonly BlockData[] Blocks = new BlockData[ChunkSizeCubed];
        public Faces[] Faces = new Faces[ChunkSizeCubed];

        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkSize && y is >= 0 and < ChunkSize &&
                   z is >= 0 and < ChunkSize;
        }

        public BlockData GetBlock(int x, int y, int z)
        {
            return Blocks[x * ChunkSizeSquared + y * ChunkSize + z];
        }

        public void SetBlock(int x, int y, int z, BlockData blockData)
        {
            Blocks[x * ChunkSizeSquared + y * ChunkSize + z] = blockData;
        }

        public Faces GetFaces(int x, int y, int z)
        {
            return Faces[x * ChunkSizeSquared + y * ChunkSize + z];
        }

        public void SetFaces(int x, int y, int z, Faces faces)
        {
            Faces[x * ChunkSizeSquared + y * ChunkSize + z] = faces;
        }
    }
}
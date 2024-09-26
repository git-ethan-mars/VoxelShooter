namespace VoxelMap
{
    public class ChunkData
    {
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;

        internal readonly BlockData[] Blocks = new BlockData[ChunkSizeCubed];
        
        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkSize && y is >= 0 and < ChunkSize &&
                   z is >= 0 and < ChunkSize;
        }

        public BlockData GetBlock(int x, int y, int z, PositionType positionType)
        {
            if (positionType == PositionType.Global)
            {
                x %= ChunkSize;
                y %= ChunkSize;
                z %= ChunkSize;
            }

            return Blocks[x * ChunkSizeSquared + y * ChunkSize + z];
        }

        public void SetBlock(int x, int y, int z, BlockData blockData, PositionType positionType)
        {
            if (positionType == PositionType.Global)
            {
                x %= ChunkSize;
                y %= ChunkSize;
                z %= ChunkSize;
            }
            
            Blocks[x * ChunkSizeSquared + y * ChunkSize + z] = blockData;
        }
    }
}
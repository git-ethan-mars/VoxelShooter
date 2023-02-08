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
    }
}
